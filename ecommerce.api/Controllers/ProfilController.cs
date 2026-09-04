using ecommerce.api.Extensions;
using ecommerce.api.Models;
using ecommerce.api.Models.DTOs;
using ecommerce.api.Models.DTOs.MyProfile;
using ecommerce.utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce.api.Controllers
{
    [Authorize]
    [Route("profil/[action]")]
    [ApiController]
    public class ProfilController : ApiCoreController
    {
        [HttpGet]
        [ActionName("my-profile")]
        public async Task<ActionResult<ApiResponse>> GetMyProfile()
        {
            //var users = userManager.Users.Select(x => new UserDto { LastName = x.LastName, FirstName = x.FirstName, Id = x.Id.ToString(), Email = x.Email }).ToList();
            //UserDto
            var user = await userManager.Users.Where(x => x.Id == User.GetUserId()).
                Select(s => new MyProfileDto { FirstName = s.FirstName, FastName = s.LastName,  Email = s.Email }).FirstOrDefaultAsync();
            if (user == null) return NotFound(new ApiResponse(statusCode: 404, message: "Kullanıcı Bulunamadı.", data: null));
            return Ok(new ApiResponse(statusCode:200, data: user));
        }

        [HttpPut]
        [ActionName("my-profile-update")]
        public async Task<ActionResult<ApiResponse>> UpdateMyProfile(EditMyProfileDto model)
        {
            var user = await userManager.FindByNameAsync(User.GetUserName());
            if (user == null) return NotFound(new ApiResponse(statusCode: 404, message: "Kullanıcı Bulunamadı.", data: null));

            var message = await UserPasswordValidationAsync(user,model.CurrentPassword,false);
            if(!string.IsNullOrEmpty(message))
            {
                return Unauthorized(new ApiResponse(statusCode: 401, message: message, displayByDefault:true, isHtmlEnabled: true));
            }

            var isEmailChaned = !user.Email!.Equals(model.Email);
            if(isEmailChaned && await CheckEmailExistsAsync(model.Email))
            {
                return BadRequest(new ApiResponse(statusCode: 400, 
                    message: $"An account has been registerd with '{model.Email}'. Please try using another email address.", 
                    displayByDefault: true));
            }

            if(!user.UserName!.Equals(model.Email.ToLower()) && await CheckEmailExistsAsync(model.Name))
            {
                return BadRequest(new ApiResponse(statusCode: 400,
                    message: $"An account has been registered with '{model.Email}'. Please try using another name (username)",
                    displayByDefault: true));
            }

            user.FirstName = model.Name;
            user.LastName = model.Name;

            if(isEmailChaned)
            {
                user.Email = model.Email;
                user.NormalizedEmail = model.Email.ToUpper();
                user.EmailConfirmed = false;
                try
                {
                    if(await SendConfirmEmailAsync(user))
                    {
                        RemoveJwtCookie();
                        return Ok(new ApiResponse(200,
                            title: "Email was changed",
                            message: "Your changes have been saved, and you've been logged out due to the email change. Please confirm your email address."));
                    }
                    return BadRequest(new ApiResponse(400, title: SM.T_EmailSentFailed,
                        message: SM.M_EmailSentFailed, displayByDefault: true));
                }
                catch (Exception ex) {
                    return BadRequest(new ApiResponse(400, title: SM.T_EmailSentFailed,
                        message: SM.M_EmailSentFailed, displayByDefault: true));
                }
            }
            else
            {
                await Context.SaveChangesAsync();
                var appUserDto = CreateAppUserDtoAsync(user);
                return Ok(new ApiResponse(200, message: "Your changes have been saved successfully.",
                    showWithToastr: true, displayByDefault: true, data: appUserDto));
            }
        }

        [HttpPut]
        [ActionName("change-password")]
        public async Task<ActionResult<ApiResponse>> ChangePassword(ChangePasswordDto model)
        {
            var user = await userManager.FindByEmailAsync(User.GetUserName());
            if (user == null) return NotFound(new ApiResponse(404, message: "Not Found", displayByDefault: true, isHtmlEnabled: true));

            var message = await UserPasswordValidationAsync(user, model.CurrentPassword, false);
            if (!string.IsNullOrEmpty(message))
            {
                return Unauthorized(new ApiResponse(401, message: message, displayByDefault: true, isHtmlEnabled: true));
            }

            var result = await userManager.ChangePasswordAsync(user,model.CurrentPassword,model.NewPassword);

            if (!result.Succeeded) return BadRequest(new ApiResponse(400));

            return Ok(new ApiResponse(200, message: "Your password has been changed successfully.", showWithToastr: true));
        }

        [HttpDelete]
        [ActionName("delete-account")]
        public async Task<ActionResult<ApiResponse>> DeleteAccount(DeleteAccountDto model)
        {
            var user = await userManager.FindByNameAsync(User.GetUserName());
            if (user == null) return NotFound();

            if(!model.Confirmation)
            {
                return BadRequest(new ApiResponse(400, message: "Please accept confirmation.", displayByDefault: true));
            }

            if (!user.UserName.Equals(model.CurrentUserName))
            {
                return BadRequest(new ApiResponse(400, message: "Invalid username. Please try again.", displayByDefault: true));
            }

            var message = await UserPasswordValidationAsync(user, model.CurrentPassword, false);
            if(!string.IsNullOrEmpty(message))
            {
                return Unauthorized(new ApiResponse(401, message: message, displayByDefault: true, isHtmlEnabled: true));
            }
            var result = await userManager.DeleteAsync(user);

            if (!result.Succeeded) return BadRequest(new ApiResponse(500));

            RemoveJwtCookie();
            return Ok(new ApiResponse(200, message: "Your user account has been permanently deleted.", displayByDefault: true));
        }
    }
}
