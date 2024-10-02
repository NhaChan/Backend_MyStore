using MyStore.DTO;
using MyStore.Response;

namespace MyStore.Services.Users
{
    public interface IUserService
    {
        Task<PagedResponse<UserResponse>> GetAllUserAsync(int page, int pageSize, string? keySearch);
        Task<AddressDTO?> GetUserAddress(string userId);
        Task<AddressDTO?> UpdateUserAddress(string userId, AddressDTO address);
    }
}
