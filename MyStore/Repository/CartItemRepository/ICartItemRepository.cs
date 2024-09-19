using MyStore.Models;
using MyStore.Repository.CommonRepository;

namespace MyStore.Repository.CartItemRepository
{
    public interface ICartItemRepository : ICommonRepository<CartItem>
    {
        Task DeleteByCartId(string userId, IEnumerable<int> productIds);
    }
}
