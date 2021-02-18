using AuthServer.Core.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Data.Configirations
{
    public class ProductConfigiration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            //validation kullanımı gibidir ve primary key olacağı buradan belirlenir
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Stock).IsRequired();

            //en fazla 18 basamak ve virgülden onra 2 basamak alıcak şekilde decimal değerinde ayarlanmasını sağlar
            builder.Property(x => x.Price).HasColumnType("decimal(18,2)");
            builder.Property(x => x.UserId).IsRequired();
        }
    }
}
