using AuthServer.Core.Configuration;
using AuthServer.Core.Model;
using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using AuthServer.Data;
using AuthServer.Data.Repositories;
using AuthServer.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SharedLibrary.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreApp.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        
        public void ConfigureServices(IServiceCollection services)
        {
            //Herhangi bir consturacture' da �lgili Interface i g�r�rse, ilgili class�n� kullan�ca��n� anlar.
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();

            //Generic olduklar� i�in typeof kullanarak eklenmelidir
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            //IGenericService 2 tane generic parametre ald��� i�in <> aras�na virg�l koyuyoruz.
            //Ald��� parametre say�s�n�n bir eksi�i kadar virg�l koymam�z gerekir
            services.AddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //DBContext alanlar�n� eklemek
            services.AddDbContext<AppDbContext>(options =>
            {
                //DB ye ba�lanmas� i�in ilgili Methoda appsettings.json 'da yazd���m�z SQlServer d���m�n� veriyoruz
                options.UseSqlServer(Configuration.GetConnectionString("SqlServer"), sqloptions =>
                {
                    //Migration yap�lacak olan k�sm� se�iyoruz. data katman�nda oldu�u i�in Data katman�n� se�tik.
                    sqloptions.MigrationsAssembly("AuthServer.Data");
                });
            });



            //Identity DB'si i�in eklenmesi gereken alanlar
            services.AddIdentity<UserApp, IdentityRole>(opt=> {
               //Email Uniq olmas� i�in true setlenir
                opt.User.RequireUniqueEmail = true;
                
                //alfanumerik olmayan(*,?,-) karakter zorunlu olmas�n diye false setlendi
                opt.Password.RequireNonAlphanumeric = false;

            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            //CustomtokenOption class�na mapplemek i�in kullan�l�r
            services.Configure<CustomTokenOption>(Configuration.GetSection("TokenOption"));
           

            services.Configure<List<Client>>(Configuration.GetSection("Clients"));

            //Authentication k�sm� i�in kullan�l�r. Birden fazla Authentication gerektiren sistemlerde mevcuttur.
            //O zaman AuthenticationSheme yerine istedi�imiz string adda belirtebiliriz.
            //�rnek olarak hem m��teri login ekran� hemde bayiler i�in farkl� giri� ekran� ve Authentication i�lemi kulland���m�z zamanlarda string ile belirtebilirz.
            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //Elimizdeki iki �emay� burda birbiri ile konu�turuyoruz.
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts=> 
            {
                //servise.Add diyerek eklemi� bir nesneden instance almka i�in kullan�l�r.
                var tokenOptions = Configuration.GetSection("TokenOption").Get<CustomTokenOption>();
                opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    //NetCoreApp' taki appsettings.json dosyas�ndaki alanlar ilgili alanlara setlenir
                    ValidIssuer = tokenOptions.Issuer,

                    //Token'daki Audince dizi oldu�u i�in s�f�r�nc� indis'ten instance ald�k
                    ValidAudience = tokenOptions.Audience[0],
                    IssuerSigningKey = SingService.GetSymmetricSecuriyKey(tokenOptions.SecurityKey),

                    //Issuer �mzas� Do�rulan�r
                    ValidateIssuerSigningKey = true,//imzay� do�rular
                    ValidateAudience = true,//Audice do�rular
                    ValidateIssuer = true,//Issuer do�rular
                    ValidateLifetime = true,//Token �mr�n� kontrol eder
                    
                    //Token �mr�ne otomatik olarak verilen s�renin 5dk fazlas�n� verir. Zero komutu o 5dk l�k default s�reyi kald�r�r.
                    //�ste�e ba�l� olarak eklenir.
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "NetCoreApp.API", Version = "v1" });
            });
        }

       
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NetCoreApp.API v1"));
            }
            //**** S�ralama �nemli *****

            //�nce HttpS g�nderilir
            app.UseHttpsRedirection();

            //Routlama yap�l�r
            app.UseRouting();

            //Authentication yap�l�r
            app.UseAuthentication();

            //Authorization yap�l�r
            app.UseAuthorization();


            //EndPoint mapleme yap�lacak �ekilde s�ralama ayarlanmal�dr. 
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
