using MyStore.DTO;
using MyStore.Request;

namespace MyStore.Services.LogHistory
{
    public interface ILogService
    {
        Task<StockReceiptDTO> CreatedLog(LogRequest request);
    }
}
