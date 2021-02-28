using AuthServer.Core.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Data.Configirations
{
    public class UserAppConfigiration : IEntityTypeConfiguration<UserApp>
    {
        public void Configure(EntityTypeBuilder<UserApp> builder)
        {
            //City alanı Maximum 50 Karakter olacak
            builder.Property(x => x.City).HasMaxLength(50);
        }
    }
}
