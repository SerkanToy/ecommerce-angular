using ecommerce.api.Data;
using ecommerce.api.Models;
using ecommerce.api.Models.Entities.Users;
using ecommerce.api.Models.Services;
using ecommerce.api.Models.Services.IServices;
using ecommerce.utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ecommerce.api.Extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<Context>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddTransient(typeof(ITokenService), typeof(TokenService));
            //builder.Services.AddCors();

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var errors = actionContext.ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value.Errors)
                    .Select(x => x.ErrorMessage).ToArray();

                    var errorResponse = new ApiResponse(400, errors: errors);
                    return new BadRequestObjectResult(errorResponse);
                };
            });

            return builder;
        }
        public static WebApplicationBuilder AddAuthentiocanServices(this WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<Data.Context>(options => options.UseSqlServer(connectionString));
            builder.Services
                .AddIdentityCore<UserApp>(opt =>
                {
                    opt.Password.RequiredLength = SD.RequiredPasswordLength;
                    opt.Password.RequireDigit = false;
                    opt.Password.RequireLowercase = false;
                    opt.Password.RequireUppercase = false;
                    opt.Password.RequireNonAlphanumeric = false;
                    opt.SignIn.RequireConfirmedEmail = true;
                    opt.SignIn.RequireConfirmedAccount = true;
                    opt.Lockout.AllowedForNewUsers = false;
                    opt.Lockout.MaxFailedAccessAttempts = SD.MaxFailedAccessAttempts;
                    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(SD.DefaultLockoutTimeSpanInDays);
                })
                .AddRoles<RoleApp>()
                .AddEntityFrameworkStores<Context>();

            builder.Services
                .AddAuthentication(aut =>
                {
                    aut.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    aut.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    aut.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddCookie(cookie =>
                {
                    cookie.Cookie.Name = SD.IdentityAppCookie;
                })
                .AddJwtBearer(jwt =>
                {
                    jwt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!)),
                        ValidateIssuer = true,
                        //ValidAudience = builder.Configuration["JWT:Audience"],
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                    jwt.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = con =>
                        {
                            //context.Response.Cookies.Append(SD.IdentityAppCookie, context.Request.Cookies[SD.IdentityAppCookie] ?? string.Empty);
                            con.Response.Headers.Add(SD.IdentityAppCookie,"true");
                            con.Token = con.Request.Cookies[SD.IdentityAppCookie];
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorization();
            


            return builder;
        }
    }
}
