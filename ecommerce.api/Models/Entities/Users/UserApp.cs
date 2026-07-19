using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

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
        [NotMapped]
        public ICollection<UserRoleBridge> Roles { get; set; }
        public string? Salt { get; set; }

        #region Audit Log
        public DateTimeOffset CreateAt { get; set; }
        public Guid CreateUserId { get; set; }
        public DateTimeOffset? UpdateAt { get; set; }
        public Guid? UpdateUserId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeleteAt { get; set; }
        public Guid? DeleteUserId { get; set; }

        public string CreateUserName => GetCreateUserName();
        public string? UpdateUserName => GetUpdateUserName();
        public string? DeleteUserName => GetDeleteUserName();

        private string GetCreateUserName()
        {
            HttpContextAccessor httpContextAccessor = new();
            var userManager = httpContextAccessor
                .HttpContext
                .RequestServices
                .GetRequiredService<UserManager<UserApp>>();

            UserApp appUser = userManager.Users.First(p => p.Id == CreateUserId);

            return appUser.LastName + " " + appUser.FirstName + " (" + appUser.Email + ")";
        }

        private string? GetUpdateUserName()
        {
            if (UpdateUserId is null) return null;

            HttpContextAccessor httpContextAccessor = new();
            var userManager = httpContextAccessor
                .HttpContext
                .RequestServices
                .GetRequiredService<UserManager<UserApp>>();

            UserApp appUser = userManager.Users.First(p => p.Id == UpdateUserId);

            return appUser.LastName + " " + appUser.FirstName + " (" + appUser.Email + ")";
        }

        private string? GetDeleteUserName()
        {
            if (UpdateUserId is null) return null;

            HttpContextAccessor httpContextAccessor = new();
            var userManager = httpContextAccessor
                .HttpContext
                .RequestServices
                .GetRequiredService<UserManager<UserApp>>();

            UserApp appUser = userManager.Users.First(p => p.Id == DeleteUserId);

            return appUser.LastName + " " + appUser.FirstName + " (" + appUser.Email + ")";
        }

        #endregion
    }
}
