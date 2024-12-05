using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.DTO;
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
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            var result = await _context.OrderDetails
            .Where(od => 
                        od.Order.DateReceived.Month == currentMonth &&
                        od.Order.DateReceived.Year == currentYear)
            .GroupBy(od => new { od.ProductName })
            .Select(g => new Product
            {
                Name = g.Key.ProductName,
                Sold = g.Sum(od => od.Quantity)
            })
            .OrderByDescending(p => p.Sold)
            .Paginate(page, pageSize)
            .ToListAsync();

            return result;
        }
    }
}
