using ecommerce.api.Data;
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

            if(!user.EmailConfirmed)
                return Unauthorized(new ApiResponse(401, title:SM.T_ConfirmEmailFirst ,message: SM.T_ConfirmEmailFirst, displayByDefault: true));

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

            /*var data = await CreateAppUserDtoAsync(user);
            return Ok(new UserAppDto {
                Name = data.Name,
                Jwt = data.Jwt,
                MfaToken = data.MfaToken
            });*/


        }

        //[Authorize]
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
        [HttpPost]
        [ActionName("logout")]
        public IActionResult Logout()
        {
            RemoveJwtCookie();
            return NoContent();
        }

        //[Authorize]
        [HttpGet]
        [ActionName("isauthenticated")]
        public async Task<ActionResult<ApiResponse>> isAuthenticated()
        {
            var veri = User.Identity?.Name;
            var dataIs = User.Identity?.IsAuthenticated == true ? true : false; // new { IsAuthenticated = User.Identity?.IsAuthenticated ?? false }
            return Ok(new ApiResponse(
                    statusCode: 200,
                    data: new IsAuthenticatedDto { IsAuthenticated = dataIs }
                ));

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
                FirstName = registerDto.FirstName,
                UserName = registerDto.Email,
                EmailConfirmed = false,
                Salt = CreatePasswordHash(registerDto.Password)
            };

            user.CreateUserId = user.Id;
            user.CreateAt = DateTimeOffset.UtcNow;

            IdentityResult result = await userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            result = await userManager.AddToRoleAsync(user, SD.UserRole);

            if (!result.Succeeded) return BadRequest(result.Errors);

            try
            {
                if(await SendConfirmEmailAsync(user))
                {
                    return Ok(new ApiResponse(
                        statusCode: 201,
                        title: SM.T_AccountCreated,
                        message: SM.M_AccountCreated
                    ));
                }
                return BadRequest(new ApiResponse(
                        statusCode: 400,
                        title: SM.T_EmailSentFailed,
                        message: SM.M_EmailSentFailed,
                        displayByDefault: true
                    ));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(
                        statusCode: 400,
                        title: SM.T_EmailSentFailed,
                        message: SM.M_EmailSentFailed,
                        displayByDefault: true
                    ));
            }

            
        }

        [HttpPut]
        [ActionName("confirm-email")]
        public async Task<ActionResult<ApiResponse>> ConfirmEmail(ConfirmEmailDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);

            if(user == null)
            {
                return Unauthorized(
                        new ApiResponse(
                            statusCode: 401,
                            title: SM.T_InvallidToken,
                            message: SM.M_InavlidToken,
                            displayByDefault: true
                        )
                    );
            }

            if (!user.IsActive)
            {
                return Unauthorized(
                        new ApiResponse(
                            statusCode: 401,
                            title: SM.T_AccountSuspended,
                            message: SM.M_AccountSuspended,
                            displayByDefault: true
                        )
                    );
            }

            if (user.EmailConfirmed == true)
            {
                return Unauthorized(
                        new ApiResponse(
                            statusCode: 400,
                            title: SM.T_AccountWasConfirmed,
                            message: SM.M_AccountWasConfirmed,
                            displayByDefault: true
                        )
                    );
            }

            var appUserToken = await Context.UserTokens.FirstOrDefaultAsync(x =>
                x.UserId == user.Id && x.Name == SD.EC && x.Value == dto.Token
                );

            if(appUserToken == null || appUserToken.Expires <= DateTime.UtcNow)
            {
                if(appUserToken != null)
                {
                    Context.UserTokens.Remove(appUserToken);
                    await Context.SaveChangesAsync();
                }

                return Unauthorized(
                        new ApiResponse(
                            statusCode: 401,
                            title: SM.T_InvallidToken,
                            message: SM.M_InavlidToken,
                            displayByDefault: true
                        )
                    );
            }

            Context.UserTokens.Remove(appUserToken);
            user.EmailConfirmed = true;
            Context.Users.Update(user);
            await Context.SaveChangesAsync();

            return Ok(new ApiResponse(
                            statusCode: 200,
                            title: SM.T_EmailConfirmed,
                            message: SM.M_EmailConfirmed,
                            displayByDefault: true
                        ));
        }

        [HttpPut]
        [ActionName("resend-confirmation-email")]
        public async Task<ActionResult<ApiResponse>> ResendConfirmationEmail(EmailDto model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);

            if(user == null)
            {
                Pauseresponse();
                return Ok(new ApiResponse(
                    statusCode: 200,
                    title: SM.T_EmailSent,
                    message: SM.M_ConfirmEmailSend
                ));
            }

            if(!user.IsActive)
            {
                return Unauthorized(new ApiResponse(
                    statusCode: 401,
                    title: SM.T_AccountSuspended,
                    message: SM.M_AccountSuspended,
                    displayByDefault: true
                ));
            }

            if (user.EmailConfirmed == true)
            {
                return BadRequest(new ApiResponse(
                    statusCode: 400,
                    title: SM.T_AccountWasConfirmed,
                    message: SM.M_AccountWasConfirmed,
                    displayByDefault: true
                ));
            }

            try
            {
                if(await SendConfirmEmailAsync(user))
                {
                    return Ok(new ApiResponse(
                    statusCode: 200,
                    title: SM.T_EmailSent,
                    message: SM.M_ConfirmEmailSend
                ));
                }
                return BadRequest(new ApiResponse(
                    statusCode: 400,
                    title: SM.T_EmailSentFailed,
                    message: SM.M_EmailSentFailed,
                    displayByDefault: true
                ));
            }
            catch (Exception ex) 
            { 
                return BadRequest(new ApiResponse(
                    statusCode: 400,
                    title: SM.T_EmailSentFailed,
                    message: SM.M_EmailSentFailed,
                    displayByDefault: true
                ));
            }
        }

        [HttpPut]
        [ActionName("forgot-username-or-password")]
        public async Task<ActionResult<ApiResponse>> ForgotUsernameOrPassword(EmailDto model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if(user == null)
            {
                Pauseresponse();
                return Ok(new ApiResponse(
                    statusCode: 200,
                    title: SM.T_EmailSent,
                    message: SM.M_ForgotUsernamePasswordSent
                ));
            }

            if (!user.IsActive)
            {
                return Unauthorized(new ApiResponse(401, title: SM.T_AccountSuspended, message: SM.M_AccountSuspended,
                    displayByDefault: true));
            }

            if(!user.EmailConfirmed)
            {
                return BadRequest(new ApiResponse(400, title: SM.T_ConfirmEmailFirst, message: SM.M_ConfirmEmailFirst,
                   displayByDefault: true));
            }

            try
            {
                if(await SendForgotUsernameOrPasswordEmail(user))
                {
                    return Ok(new ApiResponse(200, title: SM.T_EmailSent, message: SM.M_ForgotUsernamePasswordSent));
                }

                return BadRequest(new ApiResponse(400, title: SM.T_EmailSentFailed, message: SM.M_EmailSentFailed,
                   displayByDefault: true));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, title: SM.T_EmailSentFailed, message: SM.M_EmailSentFailed,
                   displayByDefault: true));
            }
        }

        [HttpPut]
        [ActionName("reset-password")]
        public async Task<ActionResult<ApiResponse>> ResetPassword(ResetPasswordDto model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new ApiResponse(
                    statusCode: 401,
                    title: SM.T_InvallidToken,
                    message: SM.M_InavlidToken,
                    displayByDefault: true
                ));
            }

            if (!user.IsActive)
            {
                return Unauthorized(new ApiResponse(401, title: SM.T_AccountSuspended, message: SM.M_AccountSuspended,
                    displayByDefault: true));
            }

            if (!user.EmailConfirmed)
            {
                return BadRequest(new ApiResponse(400, title: SM.T_ConfirmEmailFirst, message: SM.M_ConfirmEmailFirst,
                   displayByDefault: true));
            }

            var appUserToken = await Context.UserTokens.FirstOrDefaultAsync(x =>
                                                x.UserId == user.Id && x.Name == SD.FUP && x.Value == model.Token
                                        );
            if(appUserToken == null || appUserToken.Expires <= DateTime.UtcNow)
            {
                if(appUserToken != null)
                {
                    Context.RemoveRange(appUserToken);
                    await Context.SaveChangesAsync();
                }
                return Unauthorized(new ApiResponse(401, title: SM.T_InvallidToken, message: SM.M_InavlidToken,
                    displayByDefault: true));
            }
            Context.UserTokens.Remove(appUserToken); 
            Context.SaveChanges();
            await userManager.RemovePasswordAsync(user);
            await userManager.AddPasswordAsync(user, model.NewPassword);
            return Ok(new ApiResponse(200, title: SM.T_PasswordRest, message: SM.M_PasswordRest));

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

        private async Task<bool> SendForgotUsernameOrPasswordEmail(UserApp user)
        {
            var userToken = await Context.UserTokens.FirstOrDefaultAsync(x => x.UserId == user.Id && x.Name == SD.FUP);
            var tokenExpiresInMinutes = TokenExpiresInMinutes();

            if(userToken == null)
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

        private async Task<bool> SendConfirmEmailAsync(UserApp user)
        {
            var userToken = await Context.UserTokens.Where(x => x.UserId == user.Id && x.Name == SD.EC).FirstOrDefaultAsync();
            var tokenExpiresInMinutes = TokenExpiresInMinutes();

            if(userToken == null) 
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
        }

    }
}
