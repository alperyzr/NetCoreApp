using AuthServer.Core.DTOs;
using AuthServer.Core.Model;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Service
{
    class DtoMapper:Profile
    {
        //Map lemek için AutoMapper kütüphanesi kullanılır. Burada örneğin Product.cs te ilgili alanları alıp ProductDto da 
        //Sadece Client'ın göreceği kısımları göstermek için kullanılır
        public DtoMapper()
        {
            //Product ProductDto ya dönüşecek anlamına gelir. ReverseMap ise bu işlemin tam terside olabileceğini belirtir
            CreateMap<ProductDto, Product>().ReverseMap();
            CreateMap<UserAppDto, UserApp>().ReverseMap();

        }
    }
}
