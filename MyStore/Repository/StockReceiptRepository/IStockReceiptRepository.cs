using MyStore.Models;
using MyStore.Repository.CommonRepository;
using MyStore.Response;

namespace MyStore.Repository.StockReceiptRepository
{
    public interface IStockReceiptRepository : ICommonRepository<StockReceipt>
    {
        Task<IEnumerable<StatisticData>> GetExpenseByMonthYear(int year, int? month);
        Task<IEnumerable<StatisticData>> GetStatisticProductExpenseByYear(int productId, int year, int? month);
        Task<IEnumerable<StatisticProduct>> GetStatisticProductExpenseByDate(int productId, DateTime from, DateTime to);
    }
}
