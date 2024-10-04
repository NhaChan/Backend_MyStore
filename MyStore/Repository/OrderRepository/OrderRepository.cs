using MyStore.Data;
using MyStore.Models;
using MyStore.Repository.CommonRepository;

namespace MyStore.Repository.OrderRepository
{
    public class OrderRepository : CommonRepository<Order>, IOrderRepository
    {
        private readonly CompanyDBContext _dbContext;
        public OrderRepository(CompanyDBContext context) : base(context) => _dbContext = context;
    }
}
