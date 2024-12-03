using MyStore.DTO;
using MyStore.Models;
using MyStore.Request;
using MyStore.Response;

namespace MyStore.Services.LogHistory
{
    public interface ILogService
    {
        Task<StockReceiptDTO> CreatedLog(LogRequest request);
        Task<PagedResponse<LogDTO>> GetAllLog(int page, int pageSize, string? search);
        Task<IEnumerable<LogDetail>> GetLogDetails(long logId);
    }
}
