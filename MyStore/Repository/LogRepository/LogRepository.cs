using MyStore.Data;
using MyStore.Models;
using MyStore.Repository.CommonRepository;

namespace MyStore.Repository.LogRepository
{
    public class LogRepository(CompanyDBContext dBContext) : CommonRepository<Log>(dBContext), ILogRepository
    {
        private readonly CompanyDBContext _dBContext = dBContext;
    }
}
