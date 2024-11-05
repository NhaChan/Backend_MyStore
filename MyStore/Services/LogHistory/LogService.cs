using AutoMapper;
using MyStore.DTO;
using MyStore.Models;
using MyStore.Repository.LogRepository;
using MyStore.Request;

namespace MyStore.Services.LogHistory
{
    public class LogService : ILogService
    {
        private readonly IMapper _mapper;
        private readonly ILogRepository _logRepository;
        private readonly ILogDetailRepository _logDetailRepository;

        public LogService(IMapper mapper, ILogRepository logRepository, ILogDetailRepository logDetailRepository)
        {
            _mapper = mapper;
            _logDetailRepository = logDetailRepository;
            _logRepository = logRepository;
        }

        public async Task<StockReceiptDTO> CreatedLog(LogRequest request)
        {
            var log = new Log
            {
                UserId = request.UserId,
                Note = request.Note,
                Total = request.Total,
                EntryDate = request.EntryDate,
                StockReceiptId = request.StockReceiptId,

            };
            await _logRepository.AddAsync(log);

            var listLogDetail = new List<LogDetail>();

            foreach (var item in request.logProducts)
            {

                var logDetail = new LogDetail
                {
                    ProductName = item.ProductName,
                    OriginPrice = item.OriginPrice,
                    Quantity = item.Quantity,
                    LogId = log.Id,

                };
                listLogDetail.Add(logDetail);
            }
            await _logDetailRepository.AddAsync(listLogDetail);

            return _mapper.Map<StockReceiptDTO>(log);
        }
    }
}
