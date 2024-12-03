using MyStore.Models;
using MyStore.Repository.CommonRepository;

namespace MyStore.Repository.OrderRepository
{
    public interface IPaymentMethodRepository : ICommonRepository<PaymentMethod>
    {
    }
}
