using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Service
{
   public static class ObjectMapper
    {
        //Lazy classı işlemi yapar ancak memory de tutmaz. Biz istemediğimiz sürece memoryde yer kaplamaz.
        //İstediğimiz zaman çağırdığımızda memory de yer kaplamaya başlar
        private static readonly Lazy<IMapper> lazy = new Lazy<IMapper>(() =>
         {
             var config = new MapperConfiguration(cfg =>
             {
                 cfg.AddProfile<DtoMapper>();
             });
             return config.CreateMapper();
         });

        //Bu metodu çağırdığımız zaman ilgili işlem gerçekleşecek ve memory de yer tutmaya başlayacak
        public static IMapper Mapper => lazy.Value;
    }
}
