using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce.api.Models.Entities.Users
{
    public class AppUserToken : IdentityUserToken<Guid>
    {
        public DateTime Expires { get; set; }
        [NotMapped]
        public UserApp UserApp { get; set; }
    }
}
