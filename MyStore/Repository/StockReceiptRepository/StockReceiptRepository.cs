using MyStore.Data;
using MyStore.Models;
using MyStore.Repository.CommonRepository;
using System.Linq.Expressions;
using System.Linq;
using MyStore.Services;
using Microsoft.EntityFrameworkCore;
using MyStore.Response;

namespace MyStore.Repository.StockReceiptRepository
{
    public class StockReceiptRepository(CompanyDBContext dbcontext) : CommonRepository<StockReceipt>(dbcontext), IStockReceiptRepository
    {
        private readonly CompanyDBContext _dbcontext = dbcontext;

        //public async Task<IEnumerable<StatisticData>> GetExpenseByMonthYear(int year, int? month)
        //    => month == null
        //        ? await _dbcontext.StockReceipts
        //            .Where(e => e.EntryDate.Year == year)
        //            .GroupBy(e => new { e.EntryDate.Month, e.EntryDate.Year })
        //            .Select(g => new StatisticData
        //            {
        //                Time = g.Key.Month,
        //                Total = g.Sum(e => e.Total)
        //            }).ToArrayAsync()
        //        : await _dbcontext.StockReceipts
        //            .Where(e => e.EntryDate.Year == year && e.EntryDate.Month == month)
        //            .GroupBy(e => new { e.EntryDate.Day, e.EntryDate.Month })
        //            .Select(g => new StatisticData
        //            {
        //                Time = g.Key.Day,
        //                Total = g.Sum(e => e.Total)
        //            }).ToArrayAsync();

        public async Task<IEnumerable<StatisticData>> GetExpenseByMonthYear(int year, int? month)
            => month == null
                ? await _dbcontext.StockReceipts
                    .Where(e => e.EntryDate.Year == year)
                    .GroupBy(e => new { e.EntryDate.Month, e.EntryDate.Year })
                    .Select(g => new StatisticData
                    {
                        Time = g.Key.Month,
                        Total = g.Sum(e => e.Total)
                    }).ToArrayAsync()
                : await _dbcontext.StockReceipts
                    .Where(e => e.EntryDate.Year == year && e.EntryDate.Month == month)
                    .GroupBy(e => new { e.EntryDate.Day, e.EntryDate.Month })
                    .Select(g => new StatisticData
                    {
                        Time = g.Key.Day,
                        Total = g.Sum(e => e.Total)
                    }).ToArrayAsync();

        public override async Task<IEnumerable<StockReceipt>> GetPageOrderByDescendingAsync<TKey>(int page, int pageSize, Expression<Func<StockReceipt, bool>>? expression, Expression<Func<StockReceipt, TKey>> orderByDesc)
            => expression == null
                ? await _dbcontext.StockReceipts
                    .OrderByDescending(orderByDesc)
                    .Paginate(page, pageSize)
                    .Include(e => e.User)
                    .ToArrayAsync()
                : await _dbcontext.StockReceipts
                    .Where(expression)
                    .OrderByDescending(orderByDesc)
                    .Paginate(page, pageSize)
                    .Include(e => e.User).ToArrayAsync();

        public async Task<IEnumerable<StatisticProduct>> GetStatisticProductExpenseByDate(int productId, DateTime from, DateTime to)
        {
            return await _dbcontext.StockReceipts
                .Where(e => e.EntryDate >= from && e.EntryDate <= to)
                .SelectMany(r => r.StockReceiptDetails)
                .Where(p => p.ProductId == productId)
                .GroupBy(p => p.StockReceipt.EntryDate.Date)
                .Select(g => new StatisticProduct
                {
                    Time = g.Key,
                    Total = g.Sum(p => p.Quantity * p.OriginPrice)
                }).ToArrayAsync();
        }

        public async Task<IEnumerable<StatisticData>> GetStatisticProductExpenseByYear(int productId, int year, int? month)
         => month == null
                ? await _dbcontext.StockReceipts
                    .Where(e => e.EntryDate.Year == year)
                    .SelectMany(r => r.StockReceiptDetails)
                    .Where(p => p.ProductId == productId)
                    .GroupBy(p => p.StockReceipt.EntryDate.Month)
                    .Select(g => new StatisticData
                    {
                        Time = g.Key,
                        Total = g.Sum(p => p.Quantity * p.OriginPrice)
                    }).ToArrayAsync()
                : await _dbcontext.StockReceipts
                    .Where(e => e.EntryDate.Year == year && e.EntryDate.Month == month)
                    .SelectMany(r => r.StockReceiptDetails)
                    .Where(p => p.ProductId == productId)
                    .GroupBy(p => p.StockReceipt.EntryDate.Date)
                    .Select(g => new StatisticData
                    {
                        Time = g.Key.Day,
                        Total = g.Sum(p => p.Quantity * p.OriginPrice)
                    }).ToArrayAsync();
    }
}
