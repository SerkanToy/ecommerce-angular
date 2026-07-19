using System.Security.Claims;

namespace ecommerce.api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid? GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
        public static string GetUserName(this ClaimsPrincipal user)
        {
            var userNameClaim = user.FindFirst(ClaimTypes.Name)?.Value;
            return userNameClaim;
        }
        public static string GetFullName(this ClaimsPrincipal user)
        {
            var fullNameClaim = user.FindFirst("FullName")?.Value;
            return fullNameClaim;
        }
        public static string GetUserEmail(this ClaimsPrincipal user)
        {
            var userEmailClaim = user.FindFirst(ClaimTypes.Email)?.Value;
            return userEmailClaim;
        }
    }
}
