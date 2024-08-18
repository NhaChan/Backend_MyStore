using MyStore.Models;
using MyStore.Repository.CommonRepository;

namespace MyStore.Repository.ProductRepository
{
    public interface IProductRepository : ICommonRepository<Product>
    {
        Task<IEnumerable<Product>> GetPageProductAsync(int page, int pageSize, string search);
        Task<IEnumerable<Product>> GetPageProductAsync(int page, int pageSize);
        Task<int> CountAsync(string search);
        Task<Product?> GetProductAsync(int id);
    }
}
