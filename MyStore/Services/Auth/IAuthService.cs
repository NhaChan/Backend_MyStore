using Microsoft.AspNetCore.Identity;
using MyStore.DTO;
using MyStore.Request;
using MyStore.Response;

namespace MyStore.Services.Auth
{
    public interface IAuthService
    {
        Task<JwtResponse?> Login(LoginRequest request);
        Task<IdentityResult> Register(RegisterRequest request);
        Task<TokenModel> RefreshToken(TokenModel token);
        Task Logout(string userID);
        Task<bool> SendPasswordResetTokenAsync(string email);
        bool VerifyResetToken(string email, string token);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task<bool> SendTokenAsync(string email);
    }
}
