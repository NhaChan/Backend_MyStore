using MyStore.Data;
using MyStore.Models;
using MyStore.Repository.CommonRepository;

namespace MyStore.Repository.StockReceiptRepository
{
    public class StockReceiptRepository(CompanyDBContext dbcontext) : CommonRepository<StockReceipt>(dbcontext), IStockReceiptRepository
    {
        private readonly CompanyDBContext _dbcontext = dbcontext;
    }
}
