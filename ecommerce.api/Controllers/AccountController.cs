using ecommerce.api.Extensions;
using ecommerce.api.Models;
using ecommerce.api.Models.DTOs;
using ecommerce.api.Models.Entities.Users;
using ecommerce.utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace ecommerce.api.Controllers
{
    [Route("account/[action]")]
    [ApiController]
    public class AccountController : ApiCoreController
    {
        [HttpPost]
        [ActionName("login")]
        public async Task<ActionResult<ApiResponse>> Login(LoginDto loginDto)
        {

            var user = userManager.Users.Where(u => u.Email == loginDto.Email.Trim()).FirstOrDefault();

            if (user == null)
                user = userManager.Users.Where(u => u.UserName == loginDto.Email.Trim()).FirstOrDefault();

            if (user == null)
                return Unauthorized(new ApiResponse(401, message: "Kullanıcı bulunamadı", displayByDefault: true));

            if (!user.IsActive)
                return Unauthorized(new ApiResponse(401, message: SM.T_AccountSuspended, displayByDefault: true));

            var message = await UserPasswordValidationAsync(user, loginDto.Password, true);

            if (!string.IsNullOrEmpty(message))
            {
                RemoveJwtCookie();
                return Unauthorized(new ApiResponse(401, message: message, displayByDefault: true, isHtmlEnabled: true));
            }
            return Ok(new ApiResponse(
                statusCode: 200,
                message: "Giriş başarılı",
                data: await CreateAppUserDtoAsync(user)
            ));
        }

        [Authorize]
        [HttpGet]
        [ActionName("refresh-user")]
        public async Task<ActionResult<UserAppDto>> RefreshAppUser()
        {
            var user = await userManager.Users.Where(x => x.Id == User.GetUserId()).FirstOrDefaultAsync();
            if (user is null)
            {
                RemoveJwtCookie();
                return Unauthorized(new ApiResponse(401, message: "Kullanıcı bulunamadı", displayByDefault: true));
            }


            return Ok(new ApiResponse(
                statusCode: 200,
                message: "Giriş başarılı",
                data: await CreateAppUserDtoAsync(user)
            ));
        }

        [HttpGet]
        [ActionName("name-taken")]
        public async Task<IActionResult> NameTaken([FromQuery] string name)
        {
            return Ok(new { IsToken = await CheckNameExistsAsync(name) });
        }

        [HttpGet]
        [ActionName("email-taken")]
        public async Task<IActionResult> EmailTaken([FromQuery] string email)
        {
            return Ok(new { IsToken = await CheckEmailExistsAsync(email) });
        }

        [Authorize]
        [HttpGet]
        [ActionName("logout")]
        public IActionResult Logout()
        {
            RemoveJwtCookie();
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> isAuthenticated()
        {
            var data = User.Identity?.IsAuthenticated ?? false;
            return Ok(new { IsAuthenticated = data });
        }

        [HttpPost]
        [ActionName("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (await CheckEmailExistsAsync(registerDto.Email))
            {
                return BadRequest("Email adresi kayıtlı.");
            }

            if (await CheckEmailExistsAsync(registerDto.Email))
            {
                return BadRequest("Kullanıcı adı kayıtlı.");
            }

            UserApp user = new UserApp
            {
                Email = registerDto.Email,
                LastName = registerDto.LastName,
                FirstName = registerDto.FisrtName,
                UserName = registerDto.Email,
                Salt = CreatePasswordHash(registerDto.Password)
            };

            user.CreateUserId = user.Id;
            user.CreateAt = DateTimeOffset.UtcNow;

            IdentityResult result = await userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            result = await userManager.AddToRoleAsync(user, SD.UserRole);

            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok($"İşlem Başarılı");
        }


        private string CreatePasswordHash(string password)
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

    }
}
