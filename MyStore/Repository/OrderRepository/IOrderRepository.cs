using MyStore.Models;
using MyStore.Repository.CommonRepository;
using MyStore.Response;
using System.Linq.Expressions;

namespace MyStore.Repository.OrderRepository
{
    public interface IOrderRepository : ICommonRepository<Order>
    {
        Task<Order?> SingleOrdefaultAsyncInclude(Expression<Func<Order, bool>> expression);
        Task<IEnumerable<StatisticData>> GetSaleByMonthYear(int year, int? month);
    }
}
