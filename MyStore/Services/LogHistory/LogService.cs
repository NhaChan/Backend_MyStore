using AutoMapper;
using MyStore.Constant;
using MyStore.DTO;
using MyStore.Models;
using MyStore.Repository.LogRepository;
using MyStore.Request;
using MyStore.Response;
using System.Globalization;
using System.Linq.Expressions;

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

        public async Task<PagedResponse<LogDTO>> GetAllLog(int page, int pageSize, string? search)
        {
            int totalLog;
            IEnumerable<Log> logs;

            if (string.IsNullOrEmpty(search))
            {
                totalLog = await _logRepository.CountAsync();
                logs = await _logRepository.GetPageOrderByDescendingAsync(page, pageSize, null, x => x.CreatedAt);
            }
            else
            {
                //bool isLong = long.TryParse(search, out long isSearch);
                //DateTime dateSearch;
                //bool isDate = DateTime.TryParseExact(
                //    search,
                //    "HH:mm:ss dd/MM/yyyy",
                //    CultureInfo.InvariantCulture,
                //    DateTimeStyles.None,
                //    out dateSearch);

                //Expression<Func<Log, bool>> expression = e => e.Id.Equals(isSearch)
                //    || (!isLong && isDate && e.CreatedAt.Date == dateSearch.Date);

                Expression<Func<Log, bool>> expression = e => e.StockReceiptId.ToString().Contains(search);

                totalLog = await _logRepository.CountAsync(expression);
                logs = await _logRepository.GetPageOrderByDescendingAsync(page, pageSize, expression, e => e.CreatedAt);
            }

            var items = _mapper.Map<IEnumerable<LogDTO>>(logs);
            return new PagedResponse<LogDTO>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalLog,
            };
        }

        public async Task<IEnumerable<LogDetail>> GetLogDetails(long logId)
        {
            var log = await _logDetailRepository.GetAsync(e => e.LogId == logId);
            if (log != null)
            {
                return _mapper.Map<IEnumerable<LogDetail>>(log);
            }
            throw new InvalidOperationException(ErrorMessage.NOT_FOUND);
        }
    }
}
