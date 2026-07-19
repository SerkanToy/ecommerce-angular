using ecommerce.api.Models.Entities.Users;

namespace ecommerce.api.Models.Services.IServices
{
    public interface ITokenService
    {
        Task<string> CreateJWTAsync(UserApp user);
    }
}
