using ecommerce.api.Models;
using ecommerce.utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
    }
}
