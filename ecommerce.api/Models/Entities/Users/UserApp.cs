using Microsoft.AspNetCore.Identity;

namespace ecommerce.api.Models.Entities.Users
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
        public string FullName => $"{FirstName} {LastName}";        
        public ICollection<UserRoleBridge> Roles { get; set; }
        public string Salt { get; set; }

        #region Audit Log
        public DateTimeOffset CreateAt { get; set; }
        public Guid CreateUserId { get; set; }
        public DateTimeOffset? UpdateAt { get; set; }
        public Guid? UpdateUserId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeleteAt { get; set; }
        public Guid? DeleteUserId { get; set; }
        #endregion
    }
}
