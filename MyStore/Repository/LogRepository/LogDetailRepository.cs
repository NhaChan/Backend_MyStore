using MyStore.Data;
using MyStore.Models;
using MyStore.Repository.CommonRepository;

namespace MyStore.Repository.LogRepository
{
    public class LogDetailRepository(CompanyDBContext dBContext) : CommonRepository<LogDetail>(dBContext), ILogDetailRepository
    {
        private readonly CompanyDBContext _dBContext = dBContext;
    }
}
