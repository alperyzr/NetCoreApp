using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Core.DTOs
{
   public class ClientTokenDto
    {
        public string AccesToken { get; set; }
        public DateTime AccessTokenExpiration { get; set; }

    }
}
