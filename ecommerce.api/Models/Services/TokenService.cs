using ecommerce.api.Models.DTOs;
using ecommerce.api.Models.Entities.Users;
using ecommerce.api.Models.Services.IServices;
using ecommerce.utility;
using Microsoft.IdentityModel.Tokens;
using OtpNet;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ecommerce.api.Models.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration configuration;
        private readonly SymmetricSecurityKey symmetricSecurityKey;
        private readonly SymmetricSecurityKey _mfaKey;
        public TokenService(IConfiguration configuration)
        {
            this.configuration = configuration;
            symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]!));
        }
        public async Task<string> CreateJWTAsync(UserApp user)
        {
            var claim = new List<Claim> {
                new Claim("uid", user.Id.ToString()),
                new Claim("FullName", $"{user.FirstName} {user.LastName}"),
                new Claim("UserName", $"{user.UserName}"),
                new Claim("Email", user.Email)
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
    
        public async Task<string> CreateMfaToken(string username)
        {
            var claims = new List<Claim>
            {
                new Claim(SD.UserName, username)
            };

            var creds = new SigningCredentials(_mfaKey,SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(configuration["MFA:TokenExpiresInMinutes"]!)),
                SigningCredentials = creds,
                Issuer = configuration["MFA:Issuer"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public string GetUserNameFromMfaToken(string mfaToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(mfaToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _mfaKey,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = configuration["MFA:Issuer"],
                    ClockSkew = TimeSpan.Zero
                },out SecurityToken validatedToken);

                var token = (JwtSecurityToken)validatedToken;
                return token.Claims.First(x => x.Type == SD.UserName)?.Value;
            }
            catch (Exception e) 
            { 
                return e.Message;
            }            
        }

        public bool ValideteCode(string secretKey, string code)
        {
            var totp = new Totp(Base32Encoding.ToBytes(secretKey));
            return totp.VerifyTotp(code,out _, new VerificationWindow(1,1));
        }

        public QrCodeDto GenerateQrCode(string email) 
        {
            var key = KeyGeneration.GenerateRandomKey(10);
            string secret = Base32Encoding.ToString(key);
            string issuer = configuration["MFA:Issuer"];
            string uri = $"otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}&digits=6";
            return new QrCodeDto(secret, uri); 
        }
    }
}
