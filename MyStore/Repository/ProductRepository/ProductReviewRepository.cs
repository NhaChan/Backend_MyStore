using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using MyStore.Repository.CommonRepository;
using MyStore.Services;
using System.Linq.Expressions;

namespace MyStore.Repository.ProductRepository
{
    public class ProductReviewRepository(CompanyDBContext dbcontext) : CommonRepository<ProductReview>(dbcontext), IProductReviewRepository
    {
        private readonly CompanyDBContext _dbcontext = dbcontext;

        public override async Task<IEnumerable<ProductReview>> GetPagedAsync<TKey>(int page, int pageSize, Expression<Func<ProductReview, bool>>? expression, Expression<Func<ProductReview, TKey>> orderBy)
            => expression == null 
                ? await _dbcontext.ProductReviews
                    .OrderBy(orderBy)
                    .Paginate(page, pageSize)
                    .Include(e => e.User).ToArrayAsync()
                : await _dbcontext.ProductReviews
                    .Where(expression)
                    .OrderBy(orderBy)
                    .Paginate(page, pageSize)
                    .Include(e => e.User).ToArrayAsync();

        public override async Task<IEnumerable<ProductReview>> GetPageOrderByDescendingAsync<TKey>(int page, int pageSize, Expression<Func<ProductReview, bool>>? expression, Expression<Func<ProductReview, TKey>> orderByDesc)
            => expression == null
                ? await _dbcontext.ProductReviews
                    .OrderByDescending(orderByDesc)
                    .Paginate(page, pageSize)
                    .Include(e => e.User).ToArrayAsync()
                : await _dbcontext.ProductReviews
                    .Where(expression)
                    .OrderByDescending(orderByDesc)
                    .Paginate(page, pageSize)
                    .Include(e => e.User).ToArrayAsync();
    }
}
