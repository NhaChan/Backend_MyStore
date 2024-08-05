using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.Data;
using MyStore.Request;
using MyStore.Services.Users;

namespace MyStore.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllUser([FromQuery] PageRequest request)
        {
            try
            {
                return Ok(await _userService.GetAllUserAsync(request.page, request.pageSize, request.search));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
