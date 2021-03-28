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
            //Herhangi bir consturacture' da Ýlgili Interface i görürse, ilgili classýný kullanýcaðýný anlar.
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();

            //Generic olduklarý için typeof kullanarak eklenmelidir
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            //IGenericService 2 tane generic parametre aldýðý için <> arasýna virgül koyuyoruz.
            //Aldýðý parametre sayýsýnýn bir eksiði kadar virgül koymamýz gerekir
            services.AddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //DBContext alanlarýný eklemek
            services.AddDbContext<AppDbContext>(options =>
            {
                //DB ye baðlanmasý için ilgili Methoda appsettings.json 'da yazdýðýmýz SQlServer düðümünü veriyoruz
                options.UseSqlServer(Configuration.GetConnectionString("SqlServer"), sqloptions =>
                {
                    //Migration yapýlacak olan kýsmý seçiyoruz. data katmanýnda olduðu için Data katmanýný seçtik.
                    sqloptions.MigrationsAssembly("AuthServer.Data");
                });
            });



            //Identity DB'si için eklenmesi gereken alanlar
            services.AddIdentity<UserApp, IdentityRole>(opt=> {
               //Email Uniq olmasý için true setlenir
                opt.User.RequireUniqueEmail = true;
                
                //alfanumerik olmayan(*,?,-) karakter zorunlu olmasýn diye false setlendi
                opt.Password.RequireNonAlphanumeric = false;

            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            //CustomtokenOption classýna mapplemek için kullanýlýr
            services.Configure<CustomTokenOption>(Configuration.GetSection("TokenOption"));
           

            services.Configure<List<Client>>(Configuration.GetSection("Clients"));

            //Authentication kýsmý için kullanýlýr. Birden fazla Authentication gerektiren sistemlerde mevcuttur.
            //O zaman AuthenticationSheme yerine istediðimiz string adda belirtebiliriz.
            //Örnek olarak hem müþteri login ekraný hemde bayiler için farklý giriþ ekraný ve Authentication iþlemi kullandýðýmýz zamanlarda string ile belirtebilirz.
            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //Elimizdeki iki þemayý burda birbiri ile konuþturuyoruz.
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts=> 
            {
                //servise.Add diyerek eklemiþ bir nesneden instance almka için kullanýlýr.
                var tokenOptions = Configuration.GetSection("TokenOption").Get<CustomTokenOption>();
                opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    //NetCoreApp' taki appsettings.json dosyasýndaki alanlar ilgili alanlara setlenir
                    ValidIssuer = tokenOptions.Issuer,

                    //Token'daki Audince dizi olduðu için sýfýrýncý indis'ten instance aldýk
                    ValidAudience = tokenOptions.Audience[0],
                    IssuerSigningKey = SingService.GetSymmetricSecuriyKey(tokenOptions.SecurityKey),

                    //Issuer Ýmzasý Doðrulanýr
                    ValidateIssuerSigningKey = true,//imzayý doðrular
                    ValidateAudience = true,//Audice doðrular
                    ValidateIssuer = true,//Issuer doðrular
                    ValidateLifetime = true,//Token Ömrünü kontrol eder
                    
                    //Token ömrüne otomatik olarak verilen sürenin 5dk fazlasýný verir. Zero komutu o 5dk lýk default süreyi kaldýrýr.
                    //Ýsteðe baðlý olarak eklenir.
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
            //**** Sýralama Önemli *****

            //önce HttpS gönderilir
            app.UseHttpsRedirection();

            //Routlama yapýlýr
            app.UseRouting();

            //Authentication yapýlýr
            app.UseAuthentication();

            //Authorization yapýlýr
            app.UseAuthorization();


            //EndPoint mapleme yapýlacak þekilde sýralama ayarlanmalýdr. 
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
