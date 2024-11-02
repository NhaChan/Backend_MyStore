using MyStore.DTO;
using MyStore.Request;
using MyStore.Response;

namespace MyStore.Services.StockReceipts
{
    public interface IStockReceiptService
    {
        Task<StockReceiptDTO> CreateStockReceipt(string userId, StockReceiptRequest request);
        Task<PagedResponse<StockReceiptDTO>> GetAllStock(int page, int pageSize, string?search);
        Task<IEnumerable<StockReceiptDetailResponse>> GetStockDetails(long stockId);

        Task<ExpenseReponse> GetExpenseDate(DateTime from, DateTime to);
        Task<ExpenseYearMonthReponse> GetExpenseYearMonth(int year, int? month);
    }
}
