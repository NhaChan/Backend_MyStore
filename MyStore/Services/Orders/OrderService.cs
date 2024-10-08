using AutoMapper;
using MyStore.Constant;
using MyStore.DTO;
using MyStore.Enumerations;
using MyStore.Models;
using MyStore.Repository.CartItemRepository;
using MyStore.Repository.OrderRepository;
using MyStore.Repository.ProductRepository;
using MyStore.Request;
using MyStore.Response;
using MyStore.Services.Payments;
using System.Linq.Expressions;

namespace MyStore.Services.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IMapper _mapper;
        private readonly IPaymentService _paymentService;

        public OrderService(IOrderRepository orderRepository, IProductRepository productRepository, 
            ICartItemRepository cartItemRepository, IPaymentMethodRepository paymentMethodRepository, 
            IMapper mapper, IPaymentService paymentService, IOrderDetailRepository orderDetailRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _cartItemRepository = cartItemRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _mapper = mapper;
            _paymentService = paymentService;
            _orderDetailRepository = orderDetailRepository;
        }

        private double CalculateShip(double price) => price >= 400000 ? 0 : price >= 200000 ? 20000 : 40000;

        public async Task<string?> CreateOrder(string userId, OrderRequest request)
        {
            try
            {
                var now = DateTime.Now;
                var order = new Order
                {
                    UserId = userId,
                    DeliveryAddress = request.DeliveryAddress,
                    OrderDate = now,
                    Receiver = request.Receiver,
                    Total = request.Total,
                };
                var method = await _paymentService.IsActivePaymentMethod(request.PaymentMethodId)
                    ?? throw new ArgumentException(ErrorMessage.NOT_FOUND);

                order.PaymentMethodId = request.PaymentMethodId;

                await _orderRepository.AddAsync(order);

                double total = 0;

                var cartItems = await _cartItemRepository.GetAsync(e => e.UserId == userId && request.CartIds.Contains(e.ProductId));
                var listProductUpdate = new List<Product>();

                var details = cartItems.Select(cartItem =>
                {
                    double price = cartItem.Product.Price - (cartItem.Product.Price * (cartItem.Product.Discount / 100));
                    price *= cartItem.Quantity;
                    total += price;

                    cartItem.Product.Sold += cartItem.Quantity;
                    cartItem.Product.Quantity -= cartItem.Quantity;
                    listProductUpdate.Add(cartItem.Product);
                    return new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        ProductName = cartItem.Product.Name,
                        Quantity = cartItem.Quantity,
                        OriginPrice = cartItem.Product.Price,
                        Price = price,
                    };
                }).ToList();

                double shipCost = CalculateShip(total);
                order.ShippingCost = shipCost;
                total += shipCost;

                if(total != request.Total)
                {
                    throw new ArgumentException(ErrorMessage.BAD_REQUEST);
                }

                await _orderDetailRepository.AddAsync(details);
                await _productRepository.UpdateAsync(listProductUpdate);
                await _cartItemRepository.DeleteAsync(cartItems);

                if(method != PaymentMethodEnum.COD.ToString())
                {
                    return null;
                }
                return null;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<PagedResponse<OrderDTO>> GetOrderByUserId(string userId, PageRequest page)
        {
            var orders = await _orderRepository.GetPageOrderByDescendingAsync(page.page, page.pageSize, e => e.UserId == userId, x => x.CreatedAt);
            var total = await _orderRepository.CountAsync();

            var items = _mapper.Map<IEnumerable<OrderDTO>> (orders);

            return new PagedResponse<OrderDTO>
            {
                Items = items,
                TotalItems = total,
                Page = page.page,
                PageSize = page.pageSize
            };
        }

        public async Task<PagedResponse<OrderDTO>> GetAllOrder(int page, int pageSize, string? search)
        {
            int totalOrder;
            IEnumerable<Order> orders;

            if (string.IsNullOrEmpty(search))
            {
                totalOrder = await _orderRepository.CountAsync();
                orders = await _orderRepository.GetPageOrderByDescendingAsync(page, pageSize, null, x => x.CreatedAt);
            }
            else
            {
                Expression<Func<Order, bool>> expression =
                    e => e.Id.ToString().Contains(search)
                    || (e.OrderStatus != null && e.OrderStatus.Value.ToString().Contains(search));

                totalOrder = await _orderRepository.CountAsync(expression);
                orders = await _orderRepository.GetPageOrderByDescendingAsync(page, pageSize, expression, e => e.CreatedAt);
            }

            var items = _mapper.Map<IEnumerable<OrderDTO>>(orders);
            return new PagedResponse<OrderDTO>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalOrder,
            };
        }

        public Task<OrderDetailsResponse> GetOrderDetails(int id)
        {
            throw new NotImplementedException();
        }
    }
}
