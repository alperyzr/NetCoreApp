using AuthServer.Core.Configuration;
using AuthServer.Core.Model;
using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using AuthServer.Data;
using AuthServer.Data.Repositories;
using AuthServer.Service.Services;
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

            //IserviceGeneric 2 tane generic arametre ald��� i�in <> aras�na virg�l koyuyoruz
            services.AddScoped(typeof(IServiceGeneric<,>), typeof(IServiceGeneric<,>));
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

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
