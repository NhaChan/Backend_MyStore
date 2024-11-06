using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using MyStore.Repository.CommonRepository;
using MyStore.Services;
using System.Linq.Expressions;

namespace MyStore.Repository.LogRepository
{
    public class LogRepository(CompanyDBContext dBContext) : CommonRepository<Log>(dBContext), ILogRepository
    {
        private readonly CompanyDBContext _dBContext = dBContext;

        public override async Task<IEnumerable<Log>> GetPageOrderByDescendingAsync<TKey>(int page, int pageSize, Expression<Func<Log, bool>>? expression, Expression<Func<Log, TKey>> orderByDesc)
            => expression == null
                ? await _dBContext.Logs
                    .OrderByDescending(orderByDesc)
                    .Paginate(page, pageSize)
                    .Include(e => e.User)
                    .ToArrayAsync()
                : await _dBContext.Logs
                    .Where(expression)
                    .OrderByDescending(orderByDesc)
                    .Paginate(page, pageSize)
                    .Include(e => e.User).ToArrayAsync();
    }
}
