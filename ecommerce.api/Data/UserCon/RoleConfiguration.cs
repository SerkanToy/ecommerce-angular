using ecommerce.api.Models.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ecommerce.api.Data.UserCon
{
    public class RoleConfiguration : IEntityTypeConfiguration<RoleApp>
    {
        public void Configure(EntityTypeBuilder<RoleApp> builder)
        {
            builder.ToTable("Roles");
            builder.HasKey(u => u.Id);
            builder.HasMany(u => u.Roles).WithOne(y => y.Role).OnDelete(DeleteBehavior.NoAction);
            builder.HasData(roles());
        }

        private List<RoleApp> roles()
        {
            var role = new List<RoleApp>
            {
                new RoleApp { Name = "admin", NormalizedName = "ADMIN", ConcurrencyStamp = "57AA576B-CDFC-419B-8931-B87610968C26" },
                new RoleApp { Name = "normaluser", NormalizedName = "NORMALUSER", ConcurrencyStamp = "DAD5A942-2152-4B60-ACD8-0D5E60A32963" },
                new RoleApp { Name = "technicaluser", NormalizedName = "TECHNICALUSER", ConcurrencyStamp = "722F9B03-43AA-4B0A-AD14-5D1777DE506C" },
                new RoleApp { Name = "manageruser", NormalizedName = "MANAGERUSER", ConcurrencyStamp = "21E52A10-B6FE-4748-913F-25872E628423" },
                new RoleApp { Name = "presidentuser", NormalizedName = "PRESIDENTUSER", ConcurrencyStamp = "7DC15A1B-4A16-4A30-A22E-6F54FD874E46" }
            };
            return role;
        }
    }
}
