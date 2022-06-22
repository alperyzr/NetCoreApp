using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Services
{
    public static class SingService
    {
        public static SecurityKey GetSymmetricSecuriyKey(string securityKey)
        {
            //Gelen Key'i UTF-8 ile encode etmek için Kullanılır
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
        }
    }
}
