using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using MyStore.Constant;
using MyStore.DTO;
using MyStore.Enumerations;
using MyStore.Models;
using MyStore.Repository.CartItemRepository;
using MyStore.Repository.OrderRepository;
using MyStore.Repository.ProductRepository;
using MyStore.Request;
using MyStore.Response;
using MyStore.Services.Caching;
using MyStore.Services.Payments;
using Net.payOS;
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
        private readonly PayOS _payOS;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ICachingService _cache;


        public OrderService(IOrderRepository orderRepository, IProductRepository productRepository, 
            ICartItemRepository cartItemRepository, IPaymentMethodRepository paymentMethodRepository, 
            IMapper mapper, IPaymentService paymentService, IOrderDetailRepository orderDetailRepository,
            PayOS payOS, IServiceScopeFactory serviceScopeFactory, ICachingService cache)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _cartItemRepository = cartItemRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _mapper = mapper;
            _paymentService = paymentService;
            _orderDetailRepository = orderDetailRepository;
            _payOS = payOS;
            _serviceScopeFactory = serviceScopeFactory;
            _cache = cache;
        }

        struct OrderCache
        {
            public string Url { get; set; }
            public long OrderId { get; set; }
            public string? payos_IpAddr { get; set; }
            public string? payos_CreateDate { get; set; }
            public string? payos_OrderInfo { get; set; }
        }

        private double CalculateShip(double price) => price >= 400000 ? 0 : price >= 200000 ? 2000 : 4000;

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
                //var method = await _paymentService.IsActivePaymentMethod(request.PaymentMethodId)
                //    ?? throw new ArgumentException(ErrorMessage.NOT_FOUND);

                var method = await _paymentMethodRepository
                    .SingleOrDefaultAsync(x => x.Id == request.PaymentMethodId && x.IsActive)
                    ?? throw new ArgumentException(ErrorMessage.NOT_FOUND + " phương thức thanh toán");

                order.PaymentMethodId = request.PaymentMethodId;
                order.PaymentMethodName = method.Name;

                await _orderRepository.AddAsync(order);

                double total = 0;

                var cartItems = await _cartItemRepository.GetAsync(e => e.UserId == userId && request.CartIds.Contains(e.ProductId));
                var listProductUpdate = new List<Product>();
                //var listDetails = new List<OrderDetail>();

                var details = cartItems.Select(cartItem =>
                {
                    double price = cartItem.Product.Price - (cartItem.Product.Price * (cartItem.Product.Discount / 100));
                    price *= cartItem.Quantity;
                    total += price;

                    cartItem.Product.Sold += cartItem.Quantity;
                    cartItem.Product.Quantity -= cartItem.Quantity;
                    listProductUpdate.Add(cartItem.Product);
                    var imageUrl = cartItem.Product.Images.FirstOrDefault();
                    return new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        ProductName = cartItem.Product.Name,
                        Quantity = cartItem.Quantity,
                        OriginPrice = cartItem.Product.Price,
                        Price = price,
                        ImageUrl = imageUrl != null ? imageUrl.ImageUrl : null,
                    };
                }).ToList();

                //foreach (var cartItem in cartItems)
                //{
                //    double price = cartItem.Product.Price - (cartItem.Product.Price * (cartItem.Product.Discount / 100));
                //    price *= cartItem.Quantity;
                //    total += price;

                //    cartItem.Product.Sold += cartItem.Quantity;
                //    cartItem.Product.Quantity -= cartItem.Quantity;
                //    listProductUpdate.Add(cartItem.Product);
                //    listProductUpdate.Add(new OrderDetail
                //    {
                //        OrderId = order.Id,
                //        ProductId = cartItem.ProductId,
                //        ProductName = cartItem.Product.Name,
                //        Quantity = cartItem.Quantity,
                //        OriginPrice = cartItem.Product.Price,
                //        Price = price,
                //        ImageUrl = cartItem.Product.ImageUrl,
                //    });
                //}

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


                if(method.Name == PaymentMethodEnum.PayOS.ToString())
                {
                    var orders = new PayOSOrderInfo
                    {
                        OrderId = order.Id,
                        Amount = total,
                        Products = details.Select(e => new ProductInfo
                        {
                            Name = e.ProductName,
                            Price = e.Price,
                            Quantity = e.Quantity,
                        })
                    };

                    var paymentUrl = await _paymentService.GetPayOSURL(orders);
                    var orderCache = new OrderCache()
                    {
                        OrderId = order.Id,
                        Url = paymentUrl,
                    };

                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(15)
                    };
                    cacheOptions.RegisterPostEvictionCallback(OnPayOSDeadline, this);
                    _cache.Set("Order " + order.Id, orderCache, cacheOptions);
                    return paymentUrl;
                }
                else return null;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private async void OnPayOSDeadline(object key, object? value, EvictionReason reason, object? state)
        {
            if(value != null)
            {
                using var _scope = _serviceScopeFactory.CreateScope();
                var orderRepository = _scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                var payOS = _scope.ServiceProvider.GetRequiredService<PayOS>();

                var data = (OrderCache)value;

                var paymentInfo = await payOS.getPaymentLinkInformation(data.OrderId);
                if(paymentInfo.status == "PAID")
                {
                    var order = await orderRepository.FindAsync(data.OrderId);
                    if(order != null)
                    {
                        if(paymentInfo.amount == order.Total)
                        {
                            order.PaymentTranId = paymentInfo.id;
                            order.AmountPaid = paymentInfo.amountPaid;
                            order.OrderStatus = DeliveryStatusEnum.Confirmed;
                        }
                        else
                        {
                            order.OrderStatus = DeliveryStatusEnum.Canceled;
                        }
                        await orderRepository.UpdateAsync(order);
                    }
                }
                else if(paymentInfo.status != "CANCELLED")
                {
                    await payOS.cancelPaymentLink(data.OrderId);
                }
            }
        }

        public async Task<PagedResponse<OrderDTO>> GetOrderByUserId(string userId, PageRequest page)
        {
            var orders = await _orderRepository.GetPageOrderByDescendingAsync(page.page, page.pageSize, e => e.UserId == userId, x => x.CreatedAt);
            var total = await _orderRepository.CountAsync();

            var items = _mapper.Map<IEnumerable<OrderDTO>> (orders);
            foreach (var item in items)
            {
                var p = (await _orderDetailRepository.GetAsync(x => x.OrderId == item.Id)).FirstOrDefault()?.Product;
                item.Product = _mapper.Map<ProductDTO>(p);
            }

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
                bool isLong = long.TryParse(search, out long isSearch);

                Expression<Func<Order, bool>> expression =
                    e => e.Id.Equals(isSearch)
                    || (!isLong && e.PaymentMethodName != null && e.PaymentMethodName.Contains(search));

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

        public async Task<OrderDetailsResponse> GetOrderDetails(long orderId)
        {
            var order = await _orderRepository.SingleOrdefaultAsyncInclude(e => e.Id == orderId);
            if (order != null)
            {
                return _mapper.Map<OrderDetailsResponse>(order);
            }
            throw new InvalidOperationException(ErrorMessage.ORDER_NOT_FOUND);
        }

        public async Task<OrderDetailsResponse> GetOrderDetailUser(long orderId, string userId)
        {
            var order = await _orderRepository.SingleOrdefaultAsyncInclude(e => e.Id == orderId && e.UserId == userId);
            if (order != null)
            {
                return _mapper.Map<OrderDetailsResponse>(order);
            }
            throw new InvalidOperationException(ErrorMessage.ORDER_NOT_FOUND);
        }

        public async Task UpdateOrderStatus(long orderId, OrderStatusRequest request)
        {
            var order = await _orderRepository.SingleOrDefaultAsync(x => x.Id == orderId);
            if (order != null)
            {
                if (!order.OrderStatus.Equals(DeliveryStatusEnum.Canceled)
                    || !order.OrderStatus.Equals(DeliveryStatusEnum.Received))
                {
                    order.OrderStatus += 1;
                    await _orderRepository.UpdateAsync(order);
                }
                else throw new Exception(ErrorMessage.ERROR);
            }
            else throw new ArgumentException($"Id {orderId} " + ErrorMessage.NOT_FOUND);
        }

        public async Task CancelOrder(long orderId)
        {
            var order = await _orderRepository.SingleOrDefaultAsync(e => e.Id == orderId);
            if (order != null)
            {
                if (order.OrderStatus.Equals(DeliveryStatusEnum.Proccessing)
                    || order.OrderStatus.Equals(DeliveryStatusEnum.Confirmed))
                {
                    order.OrderStatus = DeliveryStatusEnum.Canceled;

                    _cache.Remove("Order " + orderId);
                    await _orderRepository.UpdateAsync(order);
                }
                else throw new Exception(ErrorMessage.ERROR);
            }
            else throw new ArgumentException($"Id {orderId} " + ErrorMessage.NOT_FOUND);
        }
        public async Task CancelOrder(long orderId, string userId)
        {
            var order = await _orderRepository.SingleOrDefaultAsync(e => e.Id == orderId && e.UserId == userId);
            if (order != null)
            {
                if (order.OrderStatus.Equals(DeliveryStatusEnum.Proccessing)
                    || order.OrderStatus.Equals(DeliveryStatusEnum.Confirmed))
                {
                    order.OrderStatus = DeliveryStatusEnum.Canceled;

                    _cache.Remove("Order " + orderId);
                    await _orderRepository.UpdateAsync(order);
                }
                else throw new Exception(ErrorMessage.ERROR);
            }
            else throw new ArgumentException($"Id {orderId} " + ErrorMessage.NOT_FOUND);
        }

        public async Task NextOrderStatus(long orderId)
        {
            var order = await _orderRepository.FindAsync(orderId);
            if (order != null)
            {
                if (!order.OrderStatus.Equals(DeliveryStatusEnum.Received) || !order.OrderStatus.Equals(DeliveryStatusEnum.Canceled))
                {
                    order.OrderStatus += 1;
                    await _orderRepository.UpdateAsync(order);
                }
                else throw new InvalidDataException(ErrorMessage.BAD_REQUEST);
            }
            else throw new InvalidOperationException(ErrorMessage.ORDER_NOT_FOUND);
        }
    }
}
