using ecommerce.api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ecommerce.api.Data.UserCon
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRoleBridge>
    {
        public void Configure(EntityTypeBuilder<UserRoleBridge> builder)
        {
            builder.ToTable("UserRoles");
            builder.HasKey(ur => new { ur.UserId, ur.RoleId });
                builder.HasOne(ur => ur.User)
                    .WithMany(u => u.Roles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(ur => ur.Role)
                    .WithMany(r => r.Roles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.NoAction);
        }

        /*private List<UserRole> roles()
        {
            var role = new List<UserRole>
            {
                new UserRole { UserId = "150dc89a-d3d0-40d0-9899-d71c0d04f7c7", RoleId = "59ECA662-2BCF-4DD1-AD1F-5564410AAE87" },
                new UserRole { UserId = "150dc89a-d3d0-40d0-9899-d71c0d04f7c7", RoleId = "248D42C6-9661-44CC-A474-B34316AF42FC" },
                new UserRole { UserId = "150dc89a-d3d0-40d0-9899-d71c0d04f7c7", RoleId = "3638D2AB-F208-4FA4-BC0F-F9CB158053D4" },
                new UserRole { UserId = "150dc89a-d3d0-40d0-9899-d71c0d04f7c7", RoleId = "8BF307F4-A967-4625-9039-78B8F30FFE54" },
                new UserRole { UserId = "150dc89a-d3d0-40d0-9899-d71c0d04f7c7", RoleId = "D2690BA1-1887-49E3-AFB1-07936EEE8ABF" }
            };
            return role;
        }*/
    }
}
