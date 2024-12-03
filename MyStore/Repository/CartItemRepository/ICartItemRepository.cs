using MyStore.Models;
using MyStore.Repository.CommonRepository;
using System.Linq.Expressions;

namespace MyStore.Repository.CartItemRepository
{
    public interface ICartItemRepository : ICommonRepository<CartItem>
    {
        Task DeleteByCartId(string userId, IEnumerable<int> productIds);
        Task<CartItem?> SingleOrDefaultAsync(Expression<Func<CartItem, bool>> expression);
    }
}
