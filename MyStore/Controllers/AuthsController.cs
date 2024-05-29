using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.Request;
using MyStore.Services;

namespace MyStore.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthsController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthsController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            var result = await _authService.Login(loginRequest);
            if(result != null)
            {
                return Ok(result);
            }
            else
            {
                return Unauthorized("Ten hoac mat khau khong chinh xac");
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            var result = await _authService.Register(registerRequest);
            if (result.Succeeded)
            {
                return Ok();
            }
            else return BadRequest(result.Errors);
        }



    }
}
