using AuthServer.Core.DTOs;
using AuthServer.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreApp.API.Controllers
{
    //Route Yapısı
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : CustomBaseController//CustomBaseControoler' dan türeyip direk statüs kod döndürmesi için generic sınıf
    {
        private readonly IAuthenticationService _authenticationService;
        #region ctor
        public AuthController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }
        #endregion

        //Yukarıdaki Route yapısına göre ulaşmak için
        //api/controller/action olarak yazdığımızda ilgili actiona gidecektir
        [HttpPost]
        public async Task<IActionResult> CreateToken(LoginDto loginDto)
        {
            var result = await _authenticationService.CreateTokenAsync(loginDto);
            return ActionResultInstance(result);
        }

        //asenkton methodu olmadığı için async ve await methotlarını kullanmadık.
        [HttpPost]
        public IActionResult CreateTokenByClient(ClientLoginDto clientLoginDto)
        {
            var result =  _authenticationService.CreateTokenByClient(clientLoginDto);
            return ActionResultInstance(result);

        }

        [HttpPost]
        public async Task<IActionResult> RevokeRefrefhToken(RefreshTokenDto refreshTokenDto)
        {
            var result = await _authenticationService.RevokeRefreshToken(refreshTokenDto.Token);
            return ActionResultInstance(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTokenByRefreshToken(RefreshTokenDto refreshTokenDto)
        {
            var result = await _authenticationService.CreateTokenByRefreshToken(refreshTokenDto.Token);
            return ActionResultInstance(result);
        }
    }
}
