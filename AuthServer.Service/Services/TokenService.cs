using AuthServer.Core.Configuration;
using AuthServer.Core.DTOs;
using AuthServer.Core.Model;
using AuthServer.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthServer.Service.Services
{
    //Core Katmanındaki IAuthenticationService bu token kısmını kullanıcak
    class TokenService : ITokenService
    {
        //Kullanıcı ile işlem yapılacağı için Kullanıcı çekilir
        private readonly UserManager<UserApp> _userManager;
        private readonly CustomTokenOption _tokenOption;


        //Generic olarak özellik alabilme için IOption kütüphanesi kullanılır
        public TokenService(UserManager<UserApp> userManager, IOptions<CustomTokenOption> tokenOption)
        {
            _userManager = userManager;
            _tokenOption = tokenOption.Value;
        }

        private string CreateRefreshToken()
        {
            //32 Byte lık bir alan oluşturur
            var numberByte = new Byte[32];

            // Microsoft kütüphanesidir. Guid gibi random değer oluşturur
            using var rnd = RandomNumberGenerator.Create();

            // gelen random' un byte' ına oluşturulan alans etlenir
            rnd.GetBytes(numberByte);
            return Convert.ToBase64String(numberByte);
        }

        //Kullanıcı ile ilgili Claim İşlemleri Üyelik sistemleri gerektiren Claim İşlemleri
        private IEnumerable<Claim> GetClaims(UserApp userApp, List<String> audience)
        {
            var userList = new List<Claim>
            {
                //Oluşturulan Token' da ID alanını Tutar. Token ile kimlik doğrulama yapılıyorsa ID tutmak zorunludur
                new Claim(ClaimTypes.NameIdentifier,userApp.Id),
           
                //Jwt kütüphaneisinde bu tokenda Email olacağ belşirtilir. Jwt kütüphanesi yerine string bir alanda belirtilebilir.
                //Ama genel olarak sabit kütüphaneleri kullanmak daha mantıklıdır.
                new Claim(JwtRegisteredClaimNames.Email,userApp.Email),
               
                //Token'da UserName'i tutar
                new Claim(ClaimTypes.Name,userApp.UserName),
               
                //Token'a random bir guid id değeri atar
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())

            };

            //jwt.Aud ilgili token'a istek yapılıp yapılamayacağını kontrol eder
            userList.AddRange(audience.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));

            return userList;
        }

        //Client ile ilgili Claim İşlemleri Üyelik sistemi gerektirmeyecek olan Claimler için kullanılır.
        private IEnumerable<Claim>GetClaimByClient(Client client)
        {
            var claims = new List<Claim>();
            claims.AddRange(client.Audiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString());

            //Jwt.Sub subject anlamına gelir ve kimin için oluşturulacağı seçilir
            new Claim(JwtRegisteredClaimNames.Sub, client.Id.ToString());
            return claims;
        }
        public TokenDto CreateToken(UserApp userApp)
        {
            //Bugünün üzerine AccessTokenExpiration' dan gelen değeri dakika olarak ekleyip TokenSüresi belirleniyor
            var accessTokenExpiration = DateTime.Now.AddMinutes(_tokenOption.AccessTokenExpiration);

            var refreshtokenExpiration = DateTime.Now.AddMinutes(_tokenOption.RefreshTokenExpiration);

            //Token'ın imzası için kullanılır. Simetrik olarak yaptığımız için SingServiisteki ethoddan çağrılır
            var securitykey = SingService.GetSymmetricSecuriyKey(_tokenOption.SecurityKey);
            
            //Tokenı imzalıcak securityKey algoritmasını belitmiş olduk.
            SigningCredentials singningCredentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256Signature);
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer: _tokenOption.Issuer,
                expires: accessTokenExpiration,
                notBefore: DateTime.Now,
                claims: GetClaims(userApp, _tokenOption.Audience),
                signingCredentials: singningCredentials
                );


            //Jwt Token sınıfından instance alınır.
            var handler = new JwtSecurityTokenHandler();

            //Alınan Bu instance Writetoken methodu ile değişkene atanır
            var token = handler.WriteToken(jwtSecurityToken);

            //TokenDto nesnesine döndürülür
            var tokenDto = new TokenDto
            {
                AccessToken = token,
                RefreshToken = CreateRefreshToken(),
                AccessTokenExpiration = accessTokenExpiration,
                RefreshTokenExpiration = refreshtokenExpiration
            };
            return tokenDto;
        }

        public ClientTokenDto CreateTokenByClient(Client client)
        {
            
            var accessTokenExpiration = DateTime.Now.AddMinutes(_tokenOption.AccessTokenExpiration);
            var securitykey = SingService.GetSymmetricSecuriyKey(_tokenOption.SecurityKey);
  
            SigningCredentials singningCredentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256Signature);
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer: _tokenOption.Issuer,
                expires: accessTokenExpiration,
                notBefore: DateTime.Now,
                claims:GetClaimByClient(client),
                signingCredentials: singningCredentials
                );

            var handler = new JwtSecurityTokenHandler();
            var token = handler.WriteToken(jwtSecurityToken);
           
            var tokenDto = new ClientTokenDto
            {
                AccessToken = token,              
                AccessTokenExpiration = accessTokenExpiration,                
            };
            return tokenDto;
        }
    }
}
