using MyStore.Models;
using MyStore.Repository.CommonRepository;

namespace MyStore.Repository.Users
{
    public interface IUserRepository : ICommonRepository<User>
    {
        Task<IEnumerable<User>> GetAllUserAsync(int page, int pageSize);
        Task<IEnumerable<User>> GetAllUserAsync(int page, int pageSize, string search);
        //Task<int> CountAsync();
        Task<int> CountAsync(string search);

    }
}
