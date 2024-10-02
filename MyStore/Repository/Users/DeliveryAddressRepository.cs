using MyStore.Data;
using MyStore.Models;
using MyStore.Repository.CommonRepository;

namespace MyStore.Repository.Users
{
    public class DeliveryAddressRepository(CompanyDBContext dbcontext)
        : CommonRepository<DeliveryAddress>(dbcontext), IDeliveryAdressRepository
    {
    }
}
