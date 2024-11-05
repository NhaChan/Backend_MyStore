using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Enumerations;
using MyStore.Models;
using MyStore.Repository.CommonRepository;
using MyStore.Response;
using System.Linq.Expressions;

namespace MyStore.Repository.OrderRepository
{
    public class OrderRepository : CommonRepository<Order>, IOrderRepository
    {
        private readonly CompanyDBContext _dbContext;
        public OrderRepository(CompanyDBContext context) : base(context) => _dbContext = context;

        public async Task<IEnumerable<StatisticData>> GetSaleByMonthYear(int year, int? month)
            => month == null
                ? await _dbContext.Orders
                    .Where(e => e.DateReceived.Year == year && (e.OrderStatus == DeliveryStatusEnum.Received 
                                                                || e.OrderStatus == DeliveryStatusEnum.Finish))
                    .GroupBy(e => new { e.DateReceived.Month, e.DateReceived.Year })
                    .Select(g => new StatisticData
                    {
                        Time = g.Key.Month,
                        Total = g.Sum(e => e.Total)
                    }).ToArrayAsync()
                : await _dbContext.Orders
                    .Where(e => e.DateReceived.Year == year && e.DateReceived.Month == month && (e.OrderStatus == DeliveryStatusEnum.Received
                                                                || e.OrderStatus == DeliveryStatusEnum.Finish))
                    .GroupBy(e => new { e.DateReceived.Day, e.DateReceived.Month })
                    .Select(g => new StatisticData
                    {
                        Time = g.Key.Day,
                        Total = g.Sum(e => e.Total)
                    }).ToArrayAsync();

        public async Task<IEnumerable<StatisticData>> GetStatisticProductSaleByDate(int productId, DateTime from, DateTime to)
        {
            return await _dbContext.Orders
                .Where(e => e.DateReceived >= from && e.DateReceived <= to.AddDays(1) 
                       && e.OrderStatus == DeliveryStatusEnum.Received || e.OrderStatus == DeliveryStatusEnum.Finish)
                .SelectMany(r => r.OrderDetails)
                .Where(p => p.ProductId == productId)
                .GroupBy(p => p.Order.DateReceived.Date)
                .Select(g => new StatisticData
                {
                    //Time = g.Key,
                    Total = g.Sum(p => p.Price)
                }).ToArrayAsync();
        }


        public async Task<IEnumerable<StatisticData>> GetStatisticProductSaleByYear(int productId, int year, int? month)
            => month == null
                ? await _dbContext.Orders
                    .Where(e => e.DateReceived.Year == year && (e.OrderStatus == DeliveryStatusEnum.Received
                                                                || e.OrderStatus == DeliveryStatusEnum.Finish))
                    .SelectMany(r => r.OrderDetails)
                    .Where(p => p.ProductId == productId)
                    .GroupBy(p => p.Order.DateReceived.Month)
                    .Select(g => new StatisticData
                    {
                        Time = g.Key,
                        Total = g.Sum(p => p.Price)
                    }).ToArrayAsync()
                : await _dbContext.Orders
                    .Where(e => e.DateReceived.Year == year && e.DateReceived.Month == month && (e.OrderStatus == DeliveryStatusEnum.Received
                                                                || e.OrderStatus == DeliveryStatusEnum.Finish))
                    .SelectMany(r => r.OrderDetails)
                    .Where(p => p.ProductId == productId)
                    .GroupBy(p => p.Order.DateReceived.Day)
                    .Select(g => new StatisticData
                    {
                        Time = g.Key,
                        Total = g.Sum(p => p.Price)
                    }).ToArrayAsync();

        public Task<Order?> SingleOrdefaultAsyncInclude(Expression<Func<Order, bool>> expression)
        {
            return _dbContext.Orders
                .Include(e => e.OrderDetails)
                .SingleOrDefaultAsync(expression);
        }
    }
}
