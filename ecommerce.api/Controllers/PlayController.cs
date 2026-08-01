using ecommerce.api.Models;
using ecommerce.api.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ecommerce.api.Controllers
{
    [Route("play/[action]")]
    [ApiController]
    public class PlayController : ApiCoreController
    {
        [HttpGet]
        [ActionName("get-all")]
        public async Task<ActionResult<ApiResponse>> GetAll()
        {
                      //UserDto
            return Ok(new ApiResponse(statusCode: 200, data: new PlayDto { Message = "Tüm kullanıcılar listelendi" }));
        }

    }
}
