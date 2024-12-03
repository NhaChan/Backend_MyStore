using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using MyStore.Repository.CommonRepository;
using MyStore.Services;
using System.Linq.Expressions;

namespace MyStore.Repository.ProductRepository
{
    public class ProductFavoriteRepository(CompanyDBContext dbContext) : CommonRepository<ProductFavorite>(dbContext), IProductFavoriteRepository
    {
        private readonly CompanyDBContext _dbContext = dbContext;

        public override async Task<IEnumerable<ProductFavorite>> GetPagedAsync<TKey>(int page, int pageSize, Expression<Func<ProductFavorite, bool>>? expression, Expression<Func<ProductFavorite, TKey>> orderBy)
        {
            return expression == null
                ? await _dbContext.ProductFavorites
                .Paginate(page, pageSize)
                .Include(e => e.Product)
                    .ThenInclude(e => e.Images)
                .OrderBy(orderBy)
                .ToArrayAsync()
                : await _dbContext.ProductFavorites
                .Where(expression)
                .Paginate(page, pageSize)
                .Include(e => e.Product)
                    .ThenInclude(e => e.Images)
                .OrderBy(orderBy)
                .ToArrayAsync();
        }
    }
}
