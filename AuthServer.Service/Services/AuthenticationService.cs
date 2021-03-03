using AuthServer.Core.Configuration;
using AuthServer.Core.DTOs;
using AuthServer.Core.Model;
using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Service.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        //AuthenticationService' te kullanmamız gereken diğer classlar
        private readonly List<Client> _client;
        private readonly ITokenService _tokenService;
        private readonly UserManager<UserApp> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<UserRefreshToken> _userRefreshTokenService;

        #region Ctor
        public AuthenticationService
            (
            IOptions<List<Client>> optionsClient,
            ITokenService tokenService,
            UserManager<UserApp> userManager,
            IUnitOfWork unitOfWork,
            IGenericRepository<UserRefreshToken> userRefrehtokenService
            )
        {
            _client = optionsClient.Value;
            _tokenService = tokenService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _userRefreshTokenService = userRefrehtokenService;
        }
        #endregion


        public async Task<Response<TokenDto>> CreateTokenAsync(LoginDto loginDto)
        {
            if (loginDto == null)
            {
                //LoginDto nun boş olup olmadığının kontrolü, boş ise throw dan hata döndürecek
                throw new ArgumentNullException(nameof(loginDto));
            }

            //LoginDto' daki mail kontrol edilir
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return Response<TokenDto>.Fail("E-Mail veya Şifre yanlış", 400, true);
            }

            //UserManager'da şifre kontolü methoduna user'dan dönden değeri ve loginDto;'daki şifreyi veriyoruz
            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Response<TokenDto>.Fail("E-Mail veya Şifre yanlış", 400, true);
            }

            //İf sorgularının hiçbirine girmese demekki böyle bir kullanıcı var. Artık Token oluştuabilirz.
            var token = _tokenService.CreateToken(user);
            var userRefreshtoken = await _userRefreshTokenService.Where(x => x.UserId == user.Id).SingleOrDefaultAsync();

            //RefreshToken kontrolü yapılır. null ise yeni bir refrsh token oluşturulur
            if (userRefreshtoken == null)
            {
                await _userRefreshTokenService.AddAsync(new UserRefreshToken
                {
                    UserId = user.Id,
                    Code = token.RefreshToken,
                    Expiration = token.RefreshTokenExpiration
                });
            }
            else
            {
                userRefreshtoken.Code = token.RefreshToken;
                userRefreshtoken.Expiration = token.RefreshTokenExpiration;
            }
            //Asenkron şekilde Db' ye kaydeder
            await _unitOfWork.CommitAsync();

            //Token nesnesini ve 200 status durumunu geriye döner
            return Response<TokenDto>.Success(token, 200);
        }

        public Response<ClientTokenDto> CreateTokenByClient(ClientLoginDto clientLoginDto)
        {
            var client = _client.SingleOrDefault(x => x.Id == clientLoginDto.ClientId && x.Secret == clientLoginDto.ClientSecret);
            if (client == null)
            {
                return Response<ClientTokenDto>.Fail("ClientID veya Scret hatalı", 404, true);
            }

            var token = _tokenService.CreateTokenByClient(client);
            return Response<ClientTokenDto>.Success(token, 200);
        }

        public async Task<Response<TokenDto>> CreateTokenByRefreshToken(string refreshToken)
        {
            var existRefreshToken = await _userRefreshTokenService.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();
            if (existRefreshToken == null)
            {
                return Response<TokenDto>.Fail("RefreshToken bulunamadı", 404, true);
            }
            var user = await _userManager.FindByIdAsync(existRefreshToken.UserId);
            if (user == null)
            {
                return Response<TokenDto>.Fail("UserId bulunamadı", 404, true);
            }

            var tokenDto = _tokenService.CreateToken(user);
            existRefreshToken.Code = tokenDto.RefreshToken;
            existRefreshToken.Expiration = tokenDto.RefreshTokenExpiration;

            await _unitOfWork.CommitAsync();

            return Response<TokenDto>.Success(tokenDto, 200);
        }

        public async Task<Response<NoDataDto>> RevokeRefreshToken(string refreshToken)
        {
            var existRefreshToken = await _userRefreshTokenService.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();
            if (existRefreshToken == null)
            {
                return Response<NoDataDto>.Fail("RefreshToken bulunamadı", 404, true);
            }
            _userRefreshTokenService.Remove(existRefreshToken);
            await _unitOfWork.CommitAsync();
            return Response<NoDataDto>.Success(200);
        }
    }
}
