using Microsoft.AspNetCore.Identity;
using MyStore.Request;
using MyStore.Response;

namespace MyStore.Services
{
    public interface IAuthService
    {
        Task<JwtResponse?> Login(LoginRequest request);
        Task<IdentityResult> Register(RegisterRequest request);
    }
}
