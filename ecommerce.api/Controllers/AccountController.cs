using Azure.Core;
using ecommerce.api.Data;
using ecommerce.api.Extensions;
using ecommerce.api.Models;
using ecommerce.api.Models.DTOs;
using ecommerce.api.Models.Entities.Users;
using ecommerce.utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            if (!user.EmailConfirmed)
                return Unauthorized(new ApiResponse(401, title: SM.T_ConfirmEmailFirst, message: SM.T_ConfirmEmailFirst, displayByDefault: true));


            var message = await UserPasswordValidationAsync(user, loginDto.Password, true);

            if (!string.IsNullOrEmpty(message))
            {
                RemoveJwtCookie();
                return Unauthorized(new ApiResponse(401, message: message, displayByDefault: true, isHtmlEnabled: true));
            }


            if (user.TwoFactorEnabled)
            {
                return Ok(new ApiResponse(
                            statusCode: 200,
                            message: "Giriş başarılı",
                            data: await CreateAppUserDtoAsync(user,adverb:false)
                        ));
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

        [HttpPost]
        [ActionName("mfaverify")]
        public async Task<ActionResult<UserAppDto>> MfaVerify(MfaVerifyDto dto)
        {
            var userName = serviceUnitOfWork.TokenService.GetUserNameFromMfaToken(dto.MfaToken);
            if(string.IsNullOrEmpty(userName))
            {
                RemoveJwtCookie();
                return Unauthorized(new ApiResponse(401, message: "Invalid code", displayByDefault: true)); ;
            }
            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                RemoveJwtCookie();
                return Unauthorized(new ApiResponse(401, message: "Invalid code", displayByDefault: true));
            }

            //var qr = serviceUnitOfWork.TokenService.GenerateQrCode(User.GetUserEmail());

            //bool yes = await userManager.VerifyUserTokenAsync(user, SD.Authenticator, SD.MFAS, qr.Secret);

            var userToken = await Context.UserTokens
                                .Where(x => x.UserId == user.Id && x.LoginProvider == SD.Authenticator && x.Name == SD.MFAS)
                                .FirstOrDefaultAsync();

            if (userToken == null)
            {
                RemoveJwtCookie();
                return Unauthorized(new ApiResponse(401, message: "Invalid code", displayByDefault: true));
            }

            var isValid = serviceUnitOfWork.TokenService.ValidateCode(userToken.Value!, dto.Code);

            if(!isValid)
            {
                RemoveJwtCookie();
                return Unauthorized(new ApiResponse(401, message: "Invalid code", displayByDefault: true));
            }

            return Ok(new ApiResponse(
                statusCode: 200,
                message: "İşlem Başarılı",
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
                if (await SendConfirmEmailAsync(user))
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

            if (user == null)
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

            if (appUserToken == null || appUserToken.Expires <= DateTime.UtcNow)
            {
                if (appUserToken != null)
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

            if (user == null)
            {
                PauseResponse();
                return Ok(new ApiResponse(
                    statusCode: 200,
                    title: SM.T_EmailSent,
                    message: SM.M_ConfirmEmailSend
                ));
            }

            if (!user.IsActive)
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
                if (await SendConfirmEmailAsync(user))
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

        [HttpPost]
        [ActionName("forgot-username-or-password")]
        public async Task<ActionResult<ApiResponse>> ForgotUsernameOrPassword(EmailDto model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                PauseResponse();
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

            if (!user.EmailConfirmed)
            {
                return BadRequest(new ApiResponse(400, title: SM.T_ConfirmEmailFirst, message: SM.M_ConfirmEmailFirst,
                   displayByDefault: true));
            }

            try
            {
                if (await SendForgotUsernameOrPasswordEmail(user))
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
            if (appUserToken == null || appUserToken.Expires <= DateTime.UtcNow)
            {
                if (appUserToken != null)
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


        [HttpPost("mfa-disable-request")]
        public async Task<ActionResult<ApiResponse>> MfaDisableRequest(EmailDto emailDto)
        {
            var user = await userManager.FindByEmailAsync(emailDto.Email);
            if (user == null)
            {
                PauseResponse();
                return Ok(new ApiResponse(200, title:SM.T_EmailSent, message:SM.M_MfaDisableEmailSent));
            }

            if(!user.IsActive)
            {
                return Unauthorized(new ApiResponse(401, title:SM.T_AccountSuspended, message:SM.M_AccountSuspended, displayByDefault:true));
            }

            if(!user.EmailConfirmed)
            {
                return BadRequest(new ApiResponse(400, title:SM.T_ConfirmEmailFirst, message:SM.M_ConfirmEmailFirst, displayByDefault:true));
            }

            try
            {
                if(await SendConfirmEmailAsync(user))
                {
                    return Ok(new ApiResponse(200, title:SM.T_EmailSent, message:SM.M_EmailSentFailed));
                }
                return BadRequest(new ApiResponse(400, title:SM.T_EmailSentFailed,message:SM.M_EmailSentFailed,displayByDefault:true));
            }
            catch (Exception ex) 
            {
                return BadRequest(new ApiResponse(400,title:SM.T_EmailSentFailed,message:SM.M_EmailSentFailed,displayByDefault:true));
            }
        }

        [HttpPut("mfa-disable")]
        public async Task<ActionResult<ApiResponse>> MfaDisable(MfaDisableDto model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if(user == null)
            {
                return Unauthorized(new ApiResponse(401,title:SM.T_InvallidToken,message:SM.M_InavlidToken,displayByDefault:true));
            }
            if(!user.IsActive)
            {
                return Unauthorized(new ApiResponse(401,title:SM.T_AccountSuspended,message:SM.M_AccountSuspended,displayByDefault:true));
            }
            if(!user.EmailConfirmed)
            {
                return BadRequest(new ApiResponse(400,title:SM.T_ConfirmEmailFirst,message:SM.M_ConfirmEmailFirst,displayByDefault:true));
            }

            var appUserToken = await Context.UserTokens
                .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Name == SD.MFASDisable && x.Value == model.Token);

            if(appUserToken == null || appUserToken.Expires <= DateTime.UtcNow)
            {
                if(appUserToken != null)
                {
                    Context.Remove(appUserToken);
                    await Context.SaveChangesAsync();
                }
                return Unauthorized(new ApiResponse(401,title:SM.T_InvallidToken, message:SM.M_InavlidToken,displayByDefault:true));
            }

            Context.UserTokens.Remove(appUserToken);
            var result = await userManager.SetTwoFactorEnabledAsync(user,false);
            if (!result.Succeeded) return BadRequest(new ApiResponse(400,displayByDefault:true));

            result = await userManager.RemoveAuthenticationTokenAsync(user,SD.Authenticator, SD.MFAS);
            if (!result.Succeeded) return BadRequest(new ApiResponse(400,displayByDefault:true));
            return Ok(new ApiResponse(200,message: "Multi-factor authentication disabled."));
        }

        
        private async Task<bool> SendMfaDisableemail(UserApp user)
        {
            var userToken = await Context.UserTokens
                    .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Name == SD.MFASDisable);

            var tokenExpiresInMinutes = TokenExpiresInMinutes();

            if(userToken == null)
            {
                var userTokenToAdd = new AppUserToken
                {
                    UserId = user.Id,
                    Name = SD.MFASDisable,
                    Value = SD.GenerateRandomPassword(),
                    Expires = DateTime.UtcNow.AddMinutes(tokenExpiresInMinutes),
                    LoginProvider = string.Empty
                };

                Context.UserTokens.Add(userTokenToAdd);
                userToken = userTokenToAdd;
            }
            else
            {
                userToken.Value = SD.GenerateRandomPassword();
                userToken.Expires = DateTime.UtcNow.AddMinutes(tokenExpiresInMinutes);
            }

            using StreamReader streamReader = System.IO.File.OpenText("EmailTemplates/mfa_disable.html");
            string htmlBody = streamReader.ReadToEnd();

            string messageBody = string.Format(htmlBody,GetClientUrl(),$"{user.FirstName} {user.LastName}",user.UserName,user.Email,userToken.Value,tokenExpiresInMinutes);
            var emailSend = new EmailSendDto(user.Email, "MFA Disable",messageBody);

            if(await emailService.SendEmailAsync(emailSend))
            {
                await Context.SaveChangesAsync();
                return true;
            }


            return false;
        }
    }
}
