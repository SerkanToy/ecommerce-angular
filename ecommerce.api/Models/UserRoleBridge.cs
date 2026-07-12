using Microsoft.AspNetCore.Identity;

namespace ecommerce.api.Models
{
    public class UserRoleBridge: IdentityUserRole<Guid>
    {
        public UserRoleBridge() 
        {
            CreateAt = DateTime.UtcNow;
            IsActive = true;
        }

        public UserApp User { get; set; }
        public override Guid UserId { get; set; } = default!;
        public RoleApp Role { get; set; }
        public override Guid RoleId { get; set; } = default!;
        public DateTime CreateAt { get; set; }
        public bool IsActive { get; set; }
    }
}
