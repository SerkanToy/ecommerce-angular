using ecommerce.api.Models.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ecommerce.api.Data.UserCon
{
    public class AppUserTokenConfiguration : IEntityTypeConfiguration<AppUserToken>
    {
        public void Configure(EntityTypeBuilder<AppUserToken> builder)
        {
           //builder.HasOne(u => u.UserApp).WithMany(y => y.Tokens).OnDelete(DeleteBehavior.NoAction);
        }    
    }
}
