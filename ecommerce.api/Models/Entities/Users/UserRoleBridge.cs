using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce.api.Models.Entities.Users
{
    public class UserRoleBridge: IdentityUserRole<Guid>
    {
        public UserRoleBridge() 
        {
            CreateAt = DateTime.UtcNow;
            IsActive = true;
        }
        [NotMapped]
        public UserApp? UserApp { get; set; }
        [NotMapped]
        public RoleApp? RoleApp { get; set; }        
        public DateTime CreateAt { get; set; }
        public bool IsActive { get; set; }
    }
}
