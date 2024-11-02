using MyStore.Models;
using MyStore.Repository.CommonRepository;
using MyStore.Response;

namespace MyStore.Repository.StockReceiptRepository
{
    public interface IStockReceiptRepository : ICommonRepository<StockReceipt>
    {
        Task<IEnumerable<StatisticData>> GetExpenseByMonthYear(int year, int? month);
    }
}
