using ecommerce.api.Data;
using ecommerce.api.Models.DTOs;
using ecommerce.api.Models.Entities.Users;
using ecommerce.api.Models.Services.IServices;
using ecommerce.utility;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace ecommerce.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiCoreController : ControllerBase
    {
        private Context _context;
        private UserManager<UserApp> _userManager;
        private SignInManager<UserApp> _signInManager;
        //private ITokenService _tokenService;
        private IConfiguration _config;
        private HttpContext _httpContext;
        private IServiceUnitOfWork _serviceUnitOfWork;

        protected Context Context => _context ??= HttpContext.RequestServices.GetService<Context>();
        protected UserManager<UserApp> userManager => _userManager ?? HttpContext.RequestServices.GetService(typeof(UserManager<UserApp>)) as UserManager<UserApp>;
        protected IServiceUnitOfWork serviceUnitOfWork => _serviceUnitOfWork ?? HttpContext.RequestServices.GetService(typeof(IServiceUnitOfWork)) as IServiceUnitOfWork;
        protected SignInManager<UserApp> signInManager => _signInManager ?? HttpContext.RequestServices.GetService(typeof(SignInManager<UserApp>)) as SignInManager<UserApp>;
        //protected ITokenService tokenService => _tokenService ?? HttpContext.RequestServices.GetService(typeof(ITokenService)) as ITokenService;
        protected IConfiguration configuration => _config ?? HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
        protected HttpContext httpContext => _httpContext ??= HttpContext;

        protected async Task<bool> CheckEmailExistsAsync(string email)
        {
            bool emailExists = await userManager.Users.AnyAsync(u => u.Email == email);
            return emailExists;
        }


        protected async Task<bool> CheckNameExistsAsync(string username)
        {
            return await userManager.Users.AnyAsync(u => u.UserName == username);
        }

        protected async Task<bool> SendConfirmEmailAsync(UserApp user)
        {
            var userToken = await _context.UserTokens.Where(x => x.UserId == user.Id && x.Name == SD.EC).FirstOrDefaultAsync();
            var tokenExpiresInMinuest = TokenExpiresInMinutes();
            if(userToken == null)
            {
                var userTokenToAdd = new AppUserToken
                {
                    UserId = user.Id,
                    Name = SD.EC,
                    Value = SD.GenerateRandomPassword(),
                    Expires = DateTime.UtcNow.AddMinutes(tokenExpiresInMinuest),
                };
                _context.UserTokens.Add(userTokenToAdd);
                userToken = userTokenToAdd;
            }
            else
            {
                userToken.Value = SD.GenerateRandomPassword();
                userToken.Expires = DateTime.UtcNow.AddMinutes(tokenExpiresInMinuest);
            }

            using StreamReader streamReader = System.IO.File.OpenText("EmailTemplates/confirm_email.html");
            string htmlBody = streamReader.ReadToEnd();
            string messageBody = string.Format(htmlBody,GetClientUrl(),user.FullName,user.Email,userToken.Value,TokenExpiresInMinutes());
            var emailSend = new EmailSendDto(user.Email, "Verify your email address", messageBody);

            if(await serviceUnitOfWork.EmailService.SendEmailAsync(emailSend))
            {
                await _context.SaveChangesAsync();
                return true;
            }
            
            return false;
        }

        protected async Task<string> UserPasswordValidationAsync(UserApp user, string password, bool lockoutOnFailure)
        {
            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                RemoveJwtCookie();
                return SD.AccountLockedMessage(user.LockoutEnd.Value.DateTime);
            }
            //SignInResult signInResult = await signInManager.CheckPasswordSignInAsync(user:user, password: password, lockoutOnFailure: lockoutOnFailure);
            var isCurrentPasswordValid = await userManager.CheckPasswordAsync(user, password);
            if (!isCurrentPasswordValid)
            {
                await userManager.AccessFailedAsync(user);
                if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
                {
                    RemoveJwtCookie();
                    return SD.AccountLockedMessage(user.LockoutEnd.Value.DateTime);
                }
                int remaining = SD.MaxFailedAccessAttempts - user.AccessFailedCount;
                return $"Invalid password. {remaining} attempts remaining before account lockout.";
            }
            user.LockoutEnd = null;
            await userManager.ResetAccessFailedCountAsync(user);
            await Context.SaveChangesAsync();
            return null;
        }

        protected async Task<UserAppDto> CreateAppUserDtoAsync(UserApp user)
        {
            RemoveJwtCookie();
            string jwt = await serviceUnitOfWork.TokenService.CreateJWTAsync(user);
            SetJWTCookie(jwt);
            //var result = await userManager.SetAuthenticationTokenAsync(user, SD.IdentityAppTokenProvider, SD.IdentityAppTokenName, jwt);

            return new UserAppDto
            {
                Name = $"{user.FirstName} {user.LastName}",
                Jwt = jwt,
                MfaToken = "",
                Email = user.Email,
            };
        }

        private void SetJWTCookie(string jwt)
        {
            var cookieOptions = new CookieOptions
            {
                //Domain = "https://localhost:7285",
                IsEssential = true,
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(int.Parse(configuration["Jwt:ExpiresInDays"]))
            };
            Response.Cookies.Append(SD.IdentityAppCookie, jwt, cookieOptions);
            //httpContext.Response.Cookies.Append(SD.IdentityAppCookie, jwt, cookieOptions);
            //Response.Cookies.Append(SD.IdentityAppCookie, jwt, cookieOptions);
        }

        protected void RemoveJwtCookie()
        {
            httpContext.Response.Cookies.Delete(SD.IdentityAppCookie);
        }

        protected int TokenExpiresInMinutes()
        {
            return int.Parse(configuration["EmailSettings:TokenExpiresInMinutes"]);
        }

        protected string GetClientUrl()
        {
            return configuration["jwt:ClientUrl"]!;
        }

        protected void Pauseresponse(double sec = 1.3)
        {
            var t = Task.Run(async delegate
            {
                await Task.Delay(TimeSpan.FromSeconds(sec));
                return 42;
            });
            t.Wait();
        }

        protected string CreatePasswordHash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            return hashed;
        }

        protected async Task<bool> SendForgotUsernameOrPasswordEmail(UserApp user)
        {
            var userToken = await Context.UserTokens.FirstOrDefaultAsync(x => x.UserId == user.Id && x.Name == SD.FUP);
            var tokenExpiresInMinutes = TokenExpiresInMinutes();

            if (userToken == null)
            {
                var userTokenAdd = new AppUserToken
                {
                    UserId = user.Id,
                    Name = SD.FUP,
                    Value = SD.GenerateRandomPassword(),
                    Expires = DateTime.UtcNow.AddMinutes(tokenExpiresInMinutes),
                    LoginProvider = string.Empty
                };
                Context.UserTokens.Add(userTokenAdd);
                userToken = userTokenAdd;
            }
            else
            {
                userToken.Value = SD.GenerateRandomPassword();
                userToken.Expires = DateTime.UtcNow.AddMinutes(tokenExpiresInMinutes);
            }

            await Context.SaveChangesAsync();

            using StreamReader streamReader = System.IO.File.OpenText("EmailTemplates/forgot_username_password.html");
            string htmlBody = streamReader.ReadToEnd();
            string messageBody = string.Format(htmlBody, GetClientUrl(), user.FirstName + " " + user.LastName, user.UserName, user.Email, userToken.Value, tokenExpiresInMinutes);
            var emailSent = new EmailSendDto(user.Email, "Forgot Username or Password", messageBody);

            return await serviceUnitOfWork.EmailService.SendEmailAsync(emailSent);
        }

        /*protected async Task<bool> SendConfirmEmailAsync(UserApp user)
        {
            var userToken = await Context.UserTokens.Where(x => x.UserId == user.Id && x.Name == SD.EC).FirstOrDefaultAsync();
            var tokenExpiresInMinutes = TokenExpiresInMinutes();

            if (userToken == null)
            {
                var userTokenAdd = new AppUserToken
                {
                    UserId = user.Id,
                    Name = SD.EC,
                    Value = SD.GenerateRandomPassword(),
                    Expires = DateTime.UtcNow.AddMinutes(tokenExpiresInMinutes),
                    LoginProvider = string.Empty
                };
                Context.UserTokens.Add(userTokenAdd);
                userToken = userTokenAdd;
            }
            else
            {
                userToken.Value = SD.GenerateRandomPassword();
                userToken.Expires = DateTime.UtcNow.AddMinutes(tokenExpiresInMinutes);
            }

            await Context.SaveChangesAsync();

            using StreamReader streamReader = System.IO.File.OpenText("EmailTemplates/confirm_email.html");
            string htmlBody = streamReader.ReadToEnd();

            string messageBody = string.Format(htmlBody, GetClientUrl(), user.FirstName + " " + user.LastName, user.UserName, user.Email,
                userToken.Value, tokenExpiresInMinutes);
            var emailSend = new EmailSendDto(user.Email, "Verify your email address", messageBody);

            return await serviceUnitOfWork.EmailService.SendEmailAsync(emailSend);
        }*/
    }
}
