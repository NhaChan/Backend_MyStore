using MyStore.Data;
using MyStore.Models;
using MyStore.Repository.CommonRepository;

namespace MyStore.Repository.CategoryRepository
{
    public class CategoryRepository : CommonRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(CompanyDBContext context) : base(context)
        {
        }
    }
}
