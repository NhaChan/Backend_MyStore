using MyStore.Models;

namespace MyStore.Repository.Users
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUserAsync(int page, int pageSize);
        Task<IEnumerable<User>> GetAllUserAsync(int page, int pageSize, string search);
        Task<int> CountAsync();
        Task<int> CountAsync(string search);

    }
}
