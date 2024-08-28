using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MyStore.DTO;
using MyStore.ErroMessage;
using MyStore.Models;
using MyStore.Request;
using MyStore.Response;
using MyStore.Services.Caching;
using MyStore.Services.SendMail;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyStore.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;
        private readonly ISendMailService _emailSender;
        private readonly ICachingService _cachingService;

        public AuthService(SignInManager<User> signInManager, UserManager<User> userManager, 
            IConfiguration config, 
            ISendMailService emailSender,
            ICachingService cachingService
            )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _config = config;
            _emailSender = emailSender;
            _cachingService = cachingService;
        }

        //private async Task<string> CreateJWT(User user, DateTime time, bool RefreshToken)
        //{
        //    var roles = await _userManager.GetRolesAsync(user);
        //    var claims = new List<Claim>
        //    {
        //        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        //        new Claim(ClaimTypes.Email, user.Email ?? ""),
        //        new Claim(ClaimTypes.Name, user.FullName ?? "")
        //    };
        //    if (RefreshToken)
        //    {
        //        claims.Add(new Claim(ClaimTypes.Version, "Refresh_Token"));
        //    }
        //    foreach (var role in roles)
        //    {
        //        claims.Add(new Claim(ClaimTypes.Role, role));
        //    }

        //    var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["JWT:Key"] ?? ""));

        //    var jwtToken = new JwtSecurityToken(
        //        issuer: _config["JWT:Issuer"],
        //        audience: _config["JWT:Audience"],
        //        claims: claims,
        //        expires: time,
        //        signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
        //        );
        //    return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        //}

        //public async Task<TokenModel> RefreshToken(TokenModel token)
        //{
        //    var parameters = new TokenValidationParameters
        //    {
        //        ValidateAudience = true,
        //        ValidateIssuer = true,
        //        ValidateLifetime = false,
        //        ValidAudience = _config["JWT:Audience"],
        //        ValidIssuer = _config["JWT:Issuer"],
        //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["JWT:Key"] ?? ""))
        //    };
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var principal = tokenHandler.ValidateToken(token.Access_token, parameters, out SecurityToken securityToken);
        //    var jwtSecurityToken = securityToken as JwtSecurityToken;

        //    var user = await _userManager.FindByIdAsync(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "");
        //    if (user == null || jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        //    {
        //        throw new SecurityTokenException("Invalid access token");
        //    }

        //    var provider = "MyStore";
        //    var name = "Refresh_Token";

        //    var access_token = await CreateJWT(user, DateTime.UtcNow.AddMinutes(6), false);
        //    var isValid = await _userManager.VerifyUserTokenAsync(user, provider, name, token.Refresh_token ?? "");
        //    if (!isValid)
        //    {
        //        throw new SecurityTokenException("Invalid refresh token");
        //    }

        //    await _userManager.RemoveAuthenticationTokenAsync(user, provider, name);
        //    var refresh_token = await _userManager.GenerateUserTokenAsync(user, provider, name);
        //    await _userManager.SetAuthenticationTokenAsync(user, provider, name, refresh_token);

        //    return new TokenModel
        //    {
        //        Access_token = access_token,
        //        Refresh_token = refresh_token
        //    };
        //}

        //public async Task<JwtResponse?> Login(LoginRequest request)
        //{
        //    var result = await _signInManager.PasswordSignInAsync(request.Username, request.Password, false, false);
        //    if (result.Succeeded)
        //    {
        //        var user = await _userManager.FindByNameAsync(request.Username);
        //        if (user != null)
        //        {
        //            var roles = await _userManager.GetRolesAsync(user);

        //            var access_token = await CreateJWT(user, DateTime.UtcNow.AddMinutes(6), false);
        //            var refresh_token = await CreateJWT(user, DateTime.UtcNow.AddDays(1), true);

        //            var provider = "MyStore";
        //            var name = "Refresh_Token";

        //            await _userManager.RemoveAuthenticationTokenAsync(user, provider, name);
        //            //var refresh_token = await _userManager.GenerateUserTokenAsync(user, provider, name);
        //            await _userManager.SetAuthenticationTokenAsync(user, provider, name, refresh_token);

        //            return new JwtResponse
        //            {
        //                Access_token = access_token,
        //                Refresh_token = refresh_token,
        //                Name = user.FullName,
        //                Roles = roles,
        //                Email = user.Email ?? "",
        //                PhoneNumber = user.PhoneNumber,

        //            };
        //        }
        //        return null;
        //    }

        //    return null;

        //}


        private async Task<string> CreateJWT(User user, bool isRefreshToken = false)
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
            if (isRefreshToken)
            {
                claims.Add(new Claim(ClaimTypes.Version, "Refresh Token"));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["JWT:Key"] ?? ""));
            var jwtToken = new JwtSecurityToken(
                    issuer: _config["JWT:Issuer"],
                    audience: _config["JWT:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(12),
                    signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }
        public async Task<JwtResponse> Login(LoginRequest request)
        {
            var result = await _signInManager.PasswordSignInAsync(request.Username, request.Password, false, false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(request.Username);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var accessToken = await CreateJWT(user);
                    var refreshToken = await CreateJWT(user, true);

                    return new JwtResponse
                    {
                        Access_token = accessToken,
                        Refresh_token = refreshToken,
                        FullName = user.FullName,
                        Roles = roles,

                    };
                }
                throw new Exception(ErrorMessage.NOT_FOUND_USER);
            }
            throw new ArgumentException(ErrorMessage.PASSWORD_ERROR);
        }

        public async Task<IdentityResult> Register(RegisterRequest request)
        {
            var isTokenValid = VerifyResetToken(request.Email, request.Token);

            if (isTokenValid)
            {
                var User = new User()
                {
                    Email = request.Email,
                    NormalizedEmail = request.Email,
                    UserName = request.Email,
                    NormalizedUserName = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    FullName = request.Name,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                return await _userManager.CreateAsync(User, request.Password);
            }
            else throw new Exception("Invalid reset token.");
        }

        public async Task Logout(string userID)
        {
            var user = await _userManager.FindByIdAsync(userID);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            var provider = "MyStore";
            var name = "Refresh_Token";
            await _userManager.RemoveAuthenticationTokenAsync(user, provider, name);
            await _userManager.UpdateSecurityStampAsync(user);
            await _signInManager.SignOutAsync();
        }

        public async Task<bool> SendPasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            var token = new Random().Next(100000, 999999).ToString();
            _cachingService.Set(email, token, TimeSpan.FromMinutes(5));

            var message = $"Your password reset code is: {token}";
            await _emailSender.SendEmailAsync(email, "Reset password", message);

            return true;
        }

        public async Task<bool> SendTokenAsync(string email)
        {
            var token = new Random().Next(100000, 999999).ToString();
            _cachingService.Set(email, token, TimeSpan.FromMinutes(5));

            var message = $"Your password reset code is: {token}";
            await _emailSender.SendEmailAsync(email, "Reset password", message);

            return true;
        }

        public bool VerifyResetToken(string email, string token)
        {
            var cachedToken = _cachingService.Get<string>(email);
            if (cachedToken == null || cachedToken != token) return false;
            else return true;
        }
        
        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var isTokenValid = VerifyResetToken(email, token);
            if (!isTokenValid) return false;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            var t = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, t, newPassword);
            await _userManager.UpdateSecurityStampAsync(user);
            _cachingService.Remove(email);
            return result.Succeeded;
        }

        public Task<TokenModel> RefreshToken(TokenModel token)
        {
            throw new NotImplementedException();
        }
    }
}
