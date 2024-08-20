using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using MyStore.Repository.CommonRepository;
using MyStore.Services;

namespace MyStore.Repository.ProductRepository
{
    public class ProductRepository : CommonRepository<Product>, IProductRepository
    {
        private readonly CompanyDBContext _dbContext;
        public ProductRepository(CompanyDBContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CountAsync(string search)
        {
            return await _dbContext.Products
                .Where(e => e.Name.Contains(search) || e.Id.ToString().Equals(search))
                .CountAsync() ;
        }

        public async Task<IEnumerable<Product>> GetPageProductAsync(int page, int pageSize, string search)
        {
            return await _dbContext.Products
                .Where(e => e.Name.Contains(search) || e.Id.ToString().Equals(search))
                .Include(e => e.Brand)
                .Include(e => e.Caterory)
                .OrderByDescending(e => e.CreatedAt)
                .Paginate(page, pageSize)
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _dbContext.Products
                .Include(e => e.Brand)
                .Include(e => e.Caterory)
                .Include(e => e.Images)
                .SingleOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Product>> GetPageProductAsync(int page, int pageSize)
        {
            return await _dbContext.Products
                    .Include(e => e.Caterory)
                    .Include(e => e.Brand)
                    .Paginate(page, pageSize)
                    .ToListAsync();
        }
    }
}
