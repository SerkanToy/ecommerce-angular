using ecommerce.api.Models.DTOs;
using ecommerce.api.Models.Entities.Users;

namespace ecommerce.api.Models.Services.IServices
{
    public interface ITokenService
    {
        Task<string> CreateJWTAsync(UserApp user);
        Task<string> CreateMfaToken(string username);
        string GetUserNameFromMfaToken(string mfaToken);
        bool ValideteCode(string secretKey, string code);
        QrCodeDto GenerateQrCode(string userName);
    }
}
