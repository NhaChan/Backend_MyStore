using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using MyStore.Repository.CommonRepository;

namespace MyStore.Repository.OrderRepository
{
    public class PaymentMethodRepository(CompanyDBContext dBContext) : CommonRepository<PaymentMethod>(dBContext), IPaymentMethodRepository
    {
        private readonly CompanyDBContext _dBContext = dBContext;

        public override async Task<IEnumerable<PaymentMethod>> GetAllAsync()
        {
            return await _dBContext.PaymentMethods.OrderBy(e => e.Id).ToListAsync();
        }
    }

}
