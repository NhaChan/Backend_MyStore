using AutoMapper;
using MyStore.Models;
using MyStore.Repository.CartItemRepository;
using MyStore.Request;
using MyStore.Response;

namespace MyStore.Services.Carts
{
    public class CartItemService : ICartItemService
    {
        private readonly ICartItemRepository _cartItemRepository;

        public CartItemService(ICartItemRepository cartItemRepository)
        {
            _cartItemRepository = cartItemRepository;
        }
        public async Task AddToCartAsync(string userId, CartItemRequest request)
        {
            try
            {
                var exitingCartItem = await _cartItemRepository.FindAsync(userId, request.ProductId);
                if (exitingCartItem != null)
                {
                    exitingCartItem.Quantity += request.Quantity;
                    await _cartItemRepository.UpdateAsync(exitingCartItem);
                }
                else
                {
                    var newCartItem = new CartItem
                    {
                        ProductId = request.ProductId,
                        UserId = userId,
                        Quantity = request.Quantity,
                    };
                    
                    await _cartItemRepository.AddAsync(newCartItem);
                }
                
            } catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }
        public async Task<IEnumerable<CartItemResponse>> GetAllCartByUserIdAsync(string userId)
        {
            try
            {
                var items = await _cartItemRepository.GetAsync(e => e.UserId == userId);
                var res = items.Select(e =>
                {
                    var imageUrl = e.Product.Images.FirstOrDefault();
                    return new CartItemResponse
                    {
                        ProductId = e.ProductId,
                        ProductName = e.Product.Name,
                        Price = e.Product.Price,
                        Discount = e.Product.Discount,
                        ImageUrl = imageUrl != null ? imageUrl.ImageUrl : null,
                        Quantity = e.Quantity,
                    };
                });
                return res;       
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task DeleteCartAsync(string userId, IEnumerable<int> productId)
        {
            try
            {
                await _cartItemRepository.DeleteByCartId(userId, productId);
            } catch(Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}
