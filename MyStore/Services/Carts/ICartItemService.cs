using MyStore.Request;
using MyStore.Response;

namespace MyStore.Services.Carts
{
    public interface ICartItemService
    {
        Task AddToCartAsync(string userId, CartItemRequest request);
        Task<IEnumerable<CartItemResponse>> GetAllCartByUserIdAsync(string userId);
        //Task DeleteCartAsync (string userId, string cartId);
        Task DeleteCartAsync(string userId, IEnumerable<int> productId);
        Task <CartItemResponse> UpdateCartItem(string userId, string cartId, UpdateCartRequest request);
    }
}
