using Microsoft.AspNetCore.Identity;
using MyStore.Repository.Users;
using MyStore.Response;
using MyStore.Models;

namespace MyStore.Services.Users
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        public UserService(UserManager<User> userManager, IUserRepository userRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
        }
        public async Task<PagedResponse<UserResponse>> GetAllUserAsync(int page, int pageSize, string? keySearch)
        {
            int totalUser;
            IEnumerable<User> users;
            if(keySearch == null)
            {
                totalUser = await _userRepository.CountAsync();
                users = await _userRepository.GetAllUserAsync(page, pageSize);
            }
            else
            {
                totalUser = await _userRepository.CountAsync();
                users = await _userRepository.GetAllUserAsync(page, pageSize, keySearch);
            }

            var items = users
                .Select(x => new UserResponse
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber
                });
            return new PagedResponse<UserResponse>
            {
                TotalItems = totalUser,
                Items = items,
                Page = page,
                PageSize = pageSize

            };
        }
    }
}