using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using MyStore.Repository.CommonRepository;
using System.Linq.Expressions;

namespace MyStore.Repository.CartItemRepository
{
    public class CartItemRepository : CommonRepository<CartItem>, ICartItemRepository
    {
        private readonly CompanyDBContext _dbContext;
        public CartItemRepository(CompanyDBContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task DeleteByCartId(string userId, IEnumerable<int> productIds)
        {
            var cartItemDelete = await _dbContext.CartItems
                .Where(e => e.UserId == userId && productIds.Contains(e.ProductId)).ToListAsync();

            _dbContext.CartItems.RemoveRange(cartItemDelete);
            await _dbContext.SaveChangesAsync();
        }

        public override async Task<IEnumerable<CartItem>> GetAsync(Expression<Func<CartItem, bool>> expression)
        {
            return await _dbContext.CartItems
                .Include(e => e.Product)
                .ThenInclude(e => e.Images)
                .Where(expression).ToListAsync();
        }
        public async Task<CartItem?> SingleOrDefaultAsync(Expression<Func<CartItem, bool>> expression)
        {
            return await _dbContext.CartItems
                .Include(e => e.Product)
                .ThenInclude(e => e.Images)
                .AsSingleQuery().SingleOrDefaultAsync(expression);
        }
    }
}
