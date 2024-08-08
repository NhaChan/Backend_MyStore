using MyStore.Data;
using MyStore.Models;
using MyStore.Repository.CommonRepository;

namespace MyStore.Repository.BrandRepository
{
    public class BrandRepository : CommonRepository<Brand>, IBrandRepository
    {
        public BrandRepository(CompanyDBContext context) : base(context)
        {
        }
    }
}
