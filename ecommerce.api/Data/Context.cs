using ecommerce.api.Models.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ecommerce.api.Data
{
    public class Context : IdentityDbContext<UserApp, RoleApp, Guid, IdentityUserClaim<Guid>, UserRoleBridge, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public Context(DbContextOptions<Context> dbContext) : base(dbContext)
        {

        }

        public DbSet<UserApp> Users { get; set; }
        public DbSet<RoleApp> Roles { get; set; }
        public DbSet<UserRoleBridge> UserRoles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {            
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(Context).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
