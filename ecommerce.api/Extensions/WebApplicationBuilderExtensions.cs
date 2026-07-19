using ecommerce.api.Data;
using ecommerce.api.Models.Entities.Users;
using ecommerce.api.Models.Services;
using ecommerce.api.Models.Services.IServices;
using ecommerce.utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

namespace ecommerce.api.Extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
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
                .AddCookie()
                .AddJwtBearer();


            builder.Services.AddScoped(typeof(ITokenService),typeof(TokenService));


            return builder;
        }
    }
}
