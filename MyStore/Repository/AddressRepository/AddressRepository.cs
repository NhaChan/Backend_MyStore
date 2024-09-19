using MyStore.Data;
using MyStore.Models;
using MyStore.Repository.CommonRepository;

namespace MyStore.Repository.AddressRepository
{
    public class AddressRepository : CommonRepository<Address>, IAddressRepository
    {
        public AddressRepository(CompanyDBContext context) : base(context)
        {
        }
    }
}
