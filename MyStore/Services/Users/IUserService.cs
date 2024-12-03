using MyStore.DTO;
using MyStore.Enumerations;
using MyStore.Request;
using MyStore.Response;

namespace MyStore.Services.Users
{
    public interface IUserService
    {
        Task<PagedResponse<UserResponse>> GetAllUserAsync(int page, int pageSize, string? keySearch, RolesEnum role);
        Task<AddressDTO?> GetUserAddress(string userId);
        Task<AddressDTO?> UpdateUserAddress(string userId, AddressDTO address);
        Task<UserDTO> GetUserInfo(string userId);
        Task<UserDTO> UpdateUserInfo(string userId, UserDTO request);
        Task AddProductFavorite(string userId, int productId);
        Task DeleteProductFavotite(string userId, int productId);
        Task<PagedResponse<ProductDTO>> GetProductFavorite(string userId, PageRequest request);
        Task<IEnumerable<int>> GetFavorites(string userId);

        Task<UserDTO> UpdateAvt(string userId, IFormFile image);

        Task<ImageDTO> GetImage(string userId);

        Task<UserDTO> AddUser(UserCreateDTO user);
        Task DeleteUser(string userId);
        Task<UserDTO> UpdateUser(string userId, UserUpdateDTO user);
        Task<UserDTO> GetUserId(string userId);

        Task LockOut(string id, DateTimeOffset? endDate);

    }
}
