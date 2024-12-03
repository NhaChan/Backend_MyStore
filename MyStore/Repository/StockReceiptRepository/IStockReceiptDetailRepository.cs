using MyStore.Models;
using MyStore.Repository.CommonRepository;
using System.Linq.Expressions;

namespace MyStore.Repository.StockReceiptRepository
{
    public interface IStockReceiptDetailRepository : ICommonRepository<StockReceiptDetail>
    {
        Task<StockReceiptDetail?> SingleOrdefaultAsyncInclude(Expression<Func<StockReceiptDetail, bool>> expression);

    }
}
