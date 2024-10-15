using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using MyStore.Repository.CommonRepository;
using System.Linq.Expressions;

namespace MyStore.Repository.OrderRepository
{
    public class OrderRepository : CommonRepository<Order>, IOrderRepository
    {
        private readonly CompanyDBContext _dbContext;
        public OrderRepository(CompanyDBContext context) : base(context) => _dbContext = context;

        public Task<Order?> SingleOrdefaultAsyncInclude(Expression<Func<Order, bool>> expression)
        {
            return _dbContext.Orders
                .Include(e => e.OrderDetails)
                .SingleOrDefaultAsync(expression);
        }
    }
}
