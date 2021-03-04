using AuthServer.Core.DTOs;
using AuthServer.Core.Model;
using AuthServer.Core.Services;
using Microsoft.AspNetCore.Identity;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Service.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<UserApp> _userManager;

        #region ctor
        public UserService(UserManager<UserApp>userManager)
        {
            _userManager = userManager;
        }
        #endregion

        public async Task<Response<UserAppDto>> CreateUserAsync(CreateUserDto userDto)
        {
            var user = new UserApp
            {
                Email = userDto.Email,
                UserName = userDto.UserName

            };

            //Identity kütüphanesi otomatik olarak parametre olarak aldığı passwordu Has ler
            var result = await _userManager.CreateAsync(user, userDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(x => x.Description).ToList();
                return Response<UserAppDto>.Fail(new ErrorDto(errors, true), 400);
            }
            //Bu method UserApp istemektedir. Object.Mapper ile UserAppDto' yu UserApp e çeviirirz
            return Response<UserAppDto>.Success(ObjectMapper.Mapper.Map<UserAppDto>(user), 200);
        }

        public async Task<Response<UserAppDto>> GetUserByNameAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return Response<UserAppDto>.Fail("UserName bulunamadı",404,true);
            }
            return Response<UserAppDto>.Success(ObjectMapper.Mapper.Map<UserAppDto>(user), 200);
        }
    }
}
