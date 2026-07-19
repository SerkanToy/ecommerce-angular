using ecommerce.api.Models.Entities.Users;
using ecommerce.api.Models.Services.IServices;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ecommerce.api.Models.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration configuration;
        private readonly SymmetricSecurityKey symmetricSecurityKey;
        public TokenService(IConfiguration configuration)
        {
            this.configuration = configuration;
            symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]!));
        }
        public async Task<string> CreateJWTAsync(UserApp user)
        {
            var claim = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("FullName", $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Name, $"{user.UserName}"),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var credentials = new SigningCredentials(symmetricSecurityKey,SecurityAlgorithms.HmacSha256);
            var tokenDescription = new SecurityTokenDescriptor { 
                Subject = new ClaimsIdentity(claim),
                Issuer = configuration["JWT:Issuer"],
                Audience = configuration["JWT:Audience"],
                Expires = DateTime.UtcNow.AddDays(int.Parse(configuration["JWT:ExpiresInDays"]!)),
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.CreateToken(tokenDescription);
            return await Task.FromResult(tokenHandler.WriteToken(jwt));
        }
    }
}
