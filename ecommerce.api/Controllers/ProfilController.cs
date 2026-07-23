using ecommerce.api.Models;
using ecommerce.api.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ecommerce.api.Controllers
{
    [Route("profil/[action]")]
    [ApiController]
    public class ProfilController : ApiCoreController
    {
        [HttpGet]
        [ActionName("get-all")]
        public async Task<ActionResult<ApiResponse>> GetAll()
        {
            var users = userManager.Users.Select(x => new UserDto { LastName = x.LastName, FirstName = x.FirstName, Id = x.Id.ToString(), Email = x.Email }).ToList();
            //UserDto
            return Ok(new ApiResponse(statusCode:200, data: users));
        }
    }
}
