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
            //Herhangi bir consturacture' da Ýlgili Interface i görürse, ilgili classýný kullanýcaðýný anlar.
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();

            //Generic olduklarý için typeof kullanarak eklenmelidir
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            //IserviceGeneric 2 tane generic arametre aldýðý için <> arasýna virgül koyuyoruz
            services.AddScoped(typeof(IServiceGeneric<,>), typeof(IServiceGeneric<,>));
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
