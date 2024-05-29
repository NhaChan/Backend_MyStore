using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MyStore.Models;
using MyStore.Request;
using MyStore.Response;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyStore.Services
{
    public class AuthService : IAuthService
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;

        public AuthService(SignInManager<User> signInManager, UserManager<User> userManager, IConfiguration config)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _config = config;
        }

        private async Task<string> CreateJWT(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.FullName ?? "")
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["JWT:Key"] ?? ""));

            var jwtToken = new JwtSecurityToken(
                issuer: _config["JWT:Issuer"],
                audience: _config["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
                );
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }

        public async Task<JwtResponse?> Login(LoginRequest request)
        {
            var result = await _signInManager.PasswordSignInAsync(request.Username, request.Password, false, false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(request.Username);
                if (user != null)
                {
                    var token = await CreateJWT(user);
                    return new JwtResponse
                    {
                        Jwt = token,
                        Name = user.FullName,
                    };
                }
                return null;
            }

            return null;

        }

        public async Task<IdentityResult> Register(RegisterRequest request)
        {
            var User = new User()
            {
                Email = request.Email,
                NormalizedEmail = request.Email,
                UserName = request.PhoneNumber,
                NormalizedUserName = request.PhoneNumber,
                PhoneNumber = request.PhoneNumber,
                FullName = request.Name,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            return await _userManager.CreateAsync(User, request.Password);
        }


    }
}
