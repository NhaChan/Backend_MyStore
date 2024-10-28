using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using MyStore.Repository.CommonRepository;
using System.Linq.Expressions;

namespace MyStore.Repository.StockReceiptRepository
{
    public class StockReceiptDetailRepository(CompanyDBContext dbcontext) 
        : CommonRepository<StockReceiptDetail>(dbcontext), IStockReceiptDetailRepository
    {
        private readonly CompanyDBContext _dbcontext = dbcontext;

        public Task<StockReceiptDetail?> SingleOrdefaultAsyncInclude(Expression<Func<StockReceiptDetail, bool>> expression)
        {
            return _dbcontext.StockReceiptDetails
                .Include(e => e.Product)
                .SingleOrDefaultAsync(expression);
        }
        public override async Task<IEnumerable<StockReceiptDetail>> GetAsync(Expression<Func<StockReceiptDetail, bool>> expression)
        {
            return await _dbcontext.StockReceiptDetails
                .Where(expression)
                .Include(e => e.Product)
                .ToListAsync();
        }
    }
}
