using ecommerce.api.Models.Entities.Users;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Security.Cryptography;

namespace ecommerce.api.Data.UserCon
{
    public class UserConfiguration : IEntityTypeConfiguration<UserApp>
    {
        public void Configure(EntityTypeBuilder<UserApp> builder)
        {
            builder.HasKey(u => u.Id);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasMany(u => u.Roles).WithOne(y => y.User).OnDelete(DeleteBehavior.NoAction);
            builder.HasData(User());
        }

        private UserApp User()
        {
            var user = new UserApp
            {
                //Id = Guid.Parse("FF4DBF3C-CE20-4E35-BEFD-1F1D89BD56D5"),
                Email = "xxx@xxx.com",
                PhoneNumber = "0(000) 000 00 00",
                LastName = "Xxxxxxx",
                FirstName = "Xxx",
                UserName = "XXX",
                NormalizedUserName = "XXX",
                NormalizedEmail = "XXX@XXX.COM",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            user.CreateUserId = user.Id;
            user.PasswordHash = CreatePasswordHash(user, "Xxx123.");
            return user;
        }

        private string CreatePasswordHash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            return hashed;
        }

        private string CreatePasswordHash(UserApp user, string password)
        {
            var passwordHash = new PasswordHasher<UserApp>();
            return passwordHash.HashPassword(user, password);
        }


    }
}
