using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace ecommerce.api.Models.Entities.Users
{
    public class RoleApp : IdentityRole<Guid>
    {
        public RoleApp()
        {
            Id = Guid.CreateVersion7(); Id = Guid.CreateVersion7();
            CreateAt = DateTime.UtcNow;
            IsActive = true;
            Roles = new List<UserRoleBridge>();
        }

        public DateTime CreateAt { get; set; }
        public bool IsActive { get; set; }
        public ICollection<UserRoleBridge> Roles { get; set; }
    }
}