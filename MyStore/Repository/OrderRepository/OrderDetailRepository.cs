using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.DTO;
using MyStore.Enumerations;
using MyStore.Models;
using MyStore.Repository.CommonRepository;
using MyStore.Services;
using System.Linq.Expressions;

namespace MyStore.Repository.OrderRepository
{
    public class OrderDetailRepository(CompanyDBContext context) : CommonRepository<OrderDetail>(context), IOrderDetailRepository
    {
        private readonly CompanyDBContext _context  = context;

        public override async Task<IEnumerable<OrderDetail>> GetAsync(Expression<Func<OrderDetail, bool>> expression)
        {
            return await _context.OrderDetails
                .Where(expression)
                .Include(e => e.Product)
                    .ThenInclude(e => e != null ? e.Images : null)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> OrderByDescendingBySoldInCurrentMonth(int page, int pageSize)
        {
            var currentMonth = DateTime.Today.Month;
            var currentYear = DateTime.Today.Year;

            var result = await _context.OrderDetails
            .Where(od => od.ProductId != null &&
                        od.Order.OrderDate.Month == currentMonth &&
                        od.Order.OrderDate.Year == currentYear && od.Order.OrderStatus != DeliveryStatusEnum.Canceled)
            .GroupBy(od => od.ProductId)
            //.GroupBy(od => new { od.ProductId, od.ProductName})
            .Select(g => new Product
            {
                Id = g.Key ?? 0,
                Name = g.FirstOrDefault() != null ? g.First().ProductName : "",
                Sold = g.Sum(od => od.Quantity)
            })
            .OrderByDescending(p => p.Sold)
            .Paginate(page, pageSize)
            .ToListAsync();

            return result;
        }

        public async Task<int> CountSold()
        {
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            var totalCount = await _context.OrderDetails
                .Where(od => od.ProductId != null &&
                    od.Order.OrderDate.Month == currentMonth &&
                    od.Order.OrderDate.Year == currentYear &&
                    od.Order.OrderStatus != DeliveryStatusEnum.Canceled)
                .GroupBy(od => od.ProductId)
                .CountAsync();

            return totalCount;
        }
    }
}
