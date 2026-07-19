using ecommerce.api.Models.Entities.Users;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce.api.Models.Entities
{
    public class Entity
    {
        #region Audit Log
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreateAt { get; set; } = DateTimeOffset.Now;
        public Guid CreateUserId { get; set; }
        public string CreateUserName => GetCreateUserName();
        public DateTimeOffset? UpdateAt { get; set; }
        public Guid? UpdateUserId { get; set; }
        public string? UpdateUserName => GetUpdateUserName();
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeleteAt { get; set; }
        [NotMapped]
        public string? DeleteUserName => GetDeleteUserName();
        public Guid? DeleteUserId { get; set; }


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
