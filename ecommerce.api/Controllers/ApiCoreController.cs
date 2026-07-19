using ecommerce.api.Data;
using ecommerce.api.Models.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ecommerce.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiCoreController : ControllerBase
    {
        private Context _context;
        private UserManager<UserApp> _userManager;
        private SignInManager<UserApp> _signInManager;
        private ITokenService _tokenService;
        private IConfiguration _config;
        private HttpContext _httpContext;

        protected Context Context => _context ??= HttpContext.RequestServices.GetService<Context>();
        protected UserManager<UserApp> userManager => _userManager ?? HttpContext.RequestServices.GetService(typeof(UserManager<UserApp>)) as UserManager<UserApp>;
        protected SignInManager<UserApp> signInManager => _signInManager ?? HttpContext.RequestServices.GetService(typeof(SignInManager<UserApp>)) as SignInManager<UserApp>;
        protected ITokenService tokenService => _tokenService ?? HttpContext.RequestServices.GetService(typeof(ITokenService)) as ITokenService;
        protected IConfiguration configuration => _config ?? HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
        protected HttpContext httpContext => _httpContext ??= HttpContext;
    }
}
