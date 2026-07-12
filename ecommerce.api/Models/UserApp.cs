using Microsoft.AspNetCore.Identity;

namespace ecommerce.api.Models
{
    public class UserApp: IdentityUser<Guid>
    {
        public UserApp()
        {
            Id = Guid.CreateVersion7();
            CreateAt = DateTime.UtcNow;
            IsActive = true;
            Roles = new List<UserRoleBridge>();
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid CreateUserId { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public DateTime CreateAt { get; set; }
        public bool IsActive { get; set; }
        public ICollection<UserRoleBridge> Roles { get; set; }
    }
}
