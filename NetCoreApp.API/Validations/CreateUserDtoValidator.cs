using AuthServer.Core.DTOs;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NetCoreApp.API.Validations
{
    //AbstractValidator classı FluentValidator ile çalışır
    public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required").EmailAddress().WithMessage("Email is wrong");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
            RuleFor(x => x.UserName).NotEmpty().WithMessage("UserName is required");

        }
    }


    //Alternatiif Kullanım Şekli (Nugget Package' tan bir şey indirmeye gerek yok)
    //public class AccountConfiguration : IEntityTypeConfiguration<CreateUserDto>
    //{
    //    public void Configure(EntityTypeBuilder<CreateUserDto> builder)
    //    {
    //        builder.Property(x => x.UserName).IsRequired();
    //        builder.Property(x => x.Password).HasColumnType("decimal(18,4)");
    //        builder.Property(x => x.Email).IsRequired();
    //        builder.HasMany(x=>x.Accounts).WithOne(x=>x.ApplicationUser).HasForeignKey(x=>x.ApplicationUserId);

    //    }
    //}
}
