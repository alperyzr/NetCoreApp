using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Core.Configuration
{
   public class Client
    {
        public string Id { get; set; }
        public string Secret { get; set; }
     
        //wwww.myapi1.com örnek sitesine erişebilir 
        public List<string> Audiences { get; set; }

    }
}
