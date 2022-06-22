using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Configuration;
using SharedLibrary.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibrary.Extensions
{
    //extension classlar static yazılmalıdır.
    public static class CustomTokenAuth
    {
        public static void AddCustomTokenAuth(this IServiceCollection services, CustomTokenOption tokenOptions)
        {
            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //Elimizdeki iki şemayı burda birbiri ile konuşturuyoruz.
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
            {
                //servise.Add diyerek eklemiş bir nesneden instance almka için kullanılır.                
                opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    //NetCoreApp' taki appsettings.json dosyasındaki alanlar ilgili alanlara setlenir
                    ValidIssuer = tokenOptions.Issuer,

                    //Token'daki Audince dizi olduğu için sıfırıncı indis'ten instance aldık
                    ValidAudience = tokenOptions.Audience[0],
                    IssuerSigningKey = SingService.GetSymmetricSecuriyKey(tokenOptions.SecurityKey),

                    //Issuer İmzası Doğrulanır
                    ValidateIssuerSigningKey = true,//imzayı doğrular
                    ValidateAudience = true,//Audice doğrular
                    ValidateIssuer = true,//Issuer doğrular
                    ValidateLifetime = true,//Token Ömrünü kontrol eder

                    //Token ömrüne otomatik olarak verilen sürenin 5dk fazlasını verir. Zero komutu o 5dk lık default süreyi kaldırır.
                    //İsteğe bağlı olarak eklenir.
                    ClockSkew = TimeSpan.Zero
                };
            });

        }
    }
}
