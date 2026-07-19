using ecommerce.api.Models.Entities.Users;
using ecommerce.api.Models.Services.IServices;

namespace ecommerce.api.Models.Services
{
    public class TokenService : ITokenService
    {
        public Task<string> CreateJWTAsync(UserApp user)
        {
            throw new NotImplementedException();
        }
    }
}
