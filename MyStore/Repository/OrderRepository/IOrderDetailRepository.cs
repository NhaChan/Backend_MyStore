using MyStore.DTO;
using MyStore.Models;
using MyStore.Repository.CommonRepository;

namespace MyStore.Repository.OrderRepository
{
    public interface IOrderDetailRepository : ICommonRepository<OrderDetail>
    {
        Task<IEnumerable<Product>> OrderByDescendingBySoldInCurrentMonth(int page, int pageSize);
        Task<int> CountSold();
    }
}
