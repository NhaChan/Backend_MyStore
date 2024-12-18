﻿using AutoMapper;
using MyStore.Constant;
using MyStore.Models;
using MyStore.Repository.CartItemRepository;
using MyStore.Repository.ProductRepository;
using MyStore.Request;
using MyStore.Response;

namespace MyStore.Services.Carts
{
    public class CartItemService : ICartItemService
    {
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;

        public CartItemService(ICartItemRepository cartItemRepository, IProductRepository productRepository)
        {
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
        }
        public async Task AddToCartAsync(string userId, CartItemRequest request)
        {
            try
            {
                var product = await _productRepository.SingleAsync(e => e.Id == request.ProductId);
                if (product != null && product.Quantity <= 0)
                {
                    throw new InvalidDataException(ErrorMessage.SOLD_OUT);
                }

                var exitingCartItem = await _cartItemRepository.FindAsyncCart(e => e.UserId == userId && e.ProductId == request.ProductId);
                if (exitingCartItem != null)
                {
                    if ((request.Quantity + exitingCartItem.Quantity) > product.Quantity)
                    {
                        throw new InvalidDataException(ErrorMessage.CART_MAX);
                    }
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
                        Stock = cartItem.Product.Quantity,
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

        public async Task<CartItemResponse> UpdateCartItem(string cartId, string userId, UpdateCartRequest request)
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

        public async Task<int> GetCount(string userId)
        {

            var cart = await _cartItemRepository.CountAsync(e => e.UserId == userId);
            return cart;
        }

        public async Task<IEnumerable<int>> GetCountProdutctId(string userId)
        {
            var cart = await _cartItemRepository.GetAsync(e => e.UserId == userId);
            return cart.Select(e => e.ProductId);
        }

        public async Task DeleteCart(string cartId, string userId)
        {
            var cartItem = await _cartItemRepository.SingleOrDefaultAsync(e => e.Id == cartId && e.UserId == userId);
            if (cartItem != null)
            {
                await _cartItemRepository.DeleteAsync(cartItem);
            }
            else throw new Exception(ErrorMessage.NOT_FOUND);
        }
    }
}
