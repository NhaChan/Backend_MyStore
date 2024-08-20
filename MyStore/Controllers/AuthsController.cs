using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.DTO;
using MyStore.Request;
using MyStore.Services.Auth;
using System.Security.Claims;

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
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(TokenModel token)
        {
            var result = await _authService.RefreshToken(token);
            if(result != null)
            {
                return Ok(result);
            }
            else 
            {
                return Unauthorized("Invalid attempt");
            };
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(string.IsNullOrEmpty(userID))
            {
                return Unauthorized("User id claim not found");
            }
            await _authService.Logout(userID);
            return Ok();
        }


        [HttpPost("send-code-resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _authService.SendPasswordResetTokenAsync(request.Email);
            if (!result)
            {
                return BadRequest("Failed to send reset token.");
            }

            return Ok("Reset token sent.");
        }

        [HttpPost("confirm-code")]
        public IActionResult VerifyResetToken([FromBody] VerifyResetTokenRequest request)
        {
            var result = _authService.VerifyResetToken(request.Email, request.Token);
            if(!result)
            {
                return BadRequest("Invalid or expired is token.");
            }
            return Ok("Reset token verified.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ConfirmResetPassword([FromBody] ConfirmResetPasswordRequest request)
        {
            var result = await _authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
            if (!result)
            {
                return BadRequest("Failed to reset password.");
            }

            return Ok("Password has been reset.");
        }

        [HttpPost("send-code-register")]
        public async Task<IActionResult> CreateToken([FromBody] ResetPasswordRequest request)
        {
            var result = await _authService.SendTokenAsync(request.Email);
            if (!result)
            {
                return BadRequest("Failed to reset password.");
            }

            return Ok("Reset token sent.");
        }

    }
}
