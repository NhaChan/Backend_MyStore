using AutoMapper;
using MyStore.Constant;
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
                var exitingCartItem = await _cartItemRepository.FindAsyncCart(e => e.UserId == userId && e.ProductId == request.ProductId);
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
                var res = items.Select(cartItem =>
                {
                    var imageUrl = cartItem.Product.Images.FirstOrDefault();
                    return new CartItemResponse
                    {
                        Id = cartItem.Id,
                        ProductId = cartItem.ProductId,
                        ProductName = cartItem.Product.Name,
                        Price = cartItem.Product.Price,
                        Discount = cartItem.Product.Discount,
                        ImageUrl = imageUrl != null ? imageUrl.ImageUrl : null,
                        Quantity = cartItem.Quantity,
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
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task<CartItemResponse> UpdateCartItem(string userId, string cartId, UpdateCartRequest request)
        {
            try
            {
                var cartItem = await _cartItemRepository.SingleOrDefaultAsync(e => e.Id == cartId && e.UserId == userId);
                if (cartItem != null)
                {
                    if (request.Quantity.HasValue)
                    {
                        cartItem.Quantity = request.Quantity.Value;
                    }
                    await _cartItemRepository.UpdateAsync(cartItem);

                    var imageUrl = cartItem.Product.Images.FirstOrDefault();
                    return new CartItemResponse
                    {
                        Id = cartItem.Id,
                        ProductId = cartItem.ProductId,
                        ProductName = cartItem.Product.Name,
                        Price = cartItem.Product.Price,
                        Discount = cartItem.Product.Discount,
                        ImageUrl = imageUrl != null ? imageUrl.ImageUrl : null,
                        Quantity = cartItem.Quantity,
                    };
                }
                throw new ArgumentException(ErrorMessage.NOT_FOUND);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
