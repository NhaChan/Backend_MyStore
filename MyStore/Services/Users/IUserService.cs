using MyStore.DTO;
using MyStore.Request;
using MyStore.Response;

namespace MyStore.Services.Users
{
    public interface IUserService
    {
        Task<PagedResponse<UserResponse>> GetAllUserAsync(int page, int pageSize, string? keySearch);
        Task<AddressDTO?> GetUserAddress(string userId);
        Task<AddressDTO?> UpdateUserAddress(string userId, AddressDTO address);
        Task<UserDTO> GetUserInfo(string userId);
        Task<UserDTO> UpdateUserInfo(string userId, UserDTO request);
        Task AddProductFavorite(string userId, int productId);
        Task DeleteProductFavotite(string userId, int productId);
        Task<PagedResponse<ProductDTO>> GetProductFavorite(string userId, PageRequest request);
    }
}
