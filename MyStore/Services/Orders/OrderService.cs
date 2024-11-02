using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using MyStore.Constant;
using MyStore.DTO;
using MyStore.Enumerations;
using MyStore.Migrations;
using MyStore.Models;
using MyStore.Repository.CartItemRepository;
using MyStore.Repository.OrderRepository;
using MyStore.Repository.ProductRepository;
using MyStore.Request;
using MyStore.Response;
using MyStore.Services.Caching;
using MyStore.Services.Payments;
using MyStore.Storage;
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
        private readonly IConfiguration _configuration;
        private readonly IFileStorage _fileStorage;
        private readonly IProductReviewRepository _productReviewRepository;

        private readonly string pathReviewImages = "assets/images/reviews";


        public OrderService(IOrderRepository orderRepository, IProductRepository productRepository, 
            ICartItemRepository cartItemRepository, IPaymentMethodRepository paymentMethodRepository, 
            IMapper mapper, IPaymentService paymentService, IOrderDetailRepository orderDetailRepository,
            PayOS payOS, IServiceScopeFactory serviceScopeFactory, ICachingService cache,
            IConfiguration configuration, IFileStorage fileStorage, IProductReviewRepository productReviewRepository)
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
            _configuration = configuration;
            _fileStorage = fileStorage;
            _productReviewRepository = productReviewRepository;
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
                    DistrictID = request.DistrictID,
                    WardID = request.WardID,
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
                    || !order.OrderStatus.Equals(DeliveryStatusEnum.Finish))
                {
                    order.OrderStatus += 1;
                    if (order.OrderStatus == DeliveryStatusEnum.Received)
                    {
                        order.DateReceived = DateTime.Now;
                    }
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
                if (!order.OrderStatus.Equals(DeliveryStatusEnum.Finish) || !order.OrderStatus.Equals(DeliveryStatusEnum.Canceled))
                {
                    order.OrderStatus += 1;
                    await _orderRepository.UpdateAsync(order);
                }
                else throw new InvalidDataException(ErrorMessage.BAD_REQUEST);
            }
            else throw new InvalidOperationException(ErrorMessage.ORDER_NOT_FOUND);
        }

        public async Task OrderToShipping(long orderId, OrderToShippingRequest request)
        {
            var order = await _orderRepository.SingleOrdefaultAsyncInclude(e => e.Id == orderId)
                ?? throw new InvalidDataException(ErrorMessage.ORDER_NOT_FOUND);
            if(order.OrderStatus != DeliveryStatusEnum.Confirmed)
            {
                throw new InvalidDataException(ErrorMessage.BAD_REQUEST);
            }

            var token = _configuration["GHN:Token"];
            var shopId = _configuration["GHN:ShopId"];
            var url = _configuration["GHN:Url"];

            var receiver = order.Receiver.Split('-').Select(e => e?.Trim()).ToArray();
            var to_name = receiver[0];
            var to_phone = receiver[1];

            if(token == null || shopId == null || url == null || to_name == null || to_phone == null)
            {
                throw new ArgumentNullException(ErrorMessage.ARGUMENT_NULL);
            }

            var to_address = order.DeliveryAddress;
            var to_ward_code = order.WardID;
            var to_district_id = order.DistrictID;

            var items = order.OrderDetails.Select(e => new
            {
                name = e.ProductName,
                quantity = e.Quantity,
                price = (int)Math.Floor(e.Price)
            }).ToArray();

            var cod_amount = order.AmountPaid < order.Total ? order.Total : 0;

            var data = new
            {
                cod_amount,
                to_name,
                to_phone,
                to_address,
                to_ward_code,
                to_district_id,
                service_type_id = 2,
                payment_type_id = 1,
                weight = request.Weight,
                length = request.Length,
                width = request.Width,
                height = request.Height,
                required_note = request.RequiredNote.ToString(),
                items
            };

            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("ShopId", shopId);
            httpClient.DefaultRequestHeaders.Add("Token", token);

            var res = await httpClient.PostAsJsonAsync(url + "/create", data);
            var dataResponse = await res.Content.ReadFromJsonAsync<GHNResponse>();
            if (!res.IsSuccessStatusCode)
            {
                throw new InvalidDataException(dataResponse?.Message ?? ErrorMessage.BAD_REQUEST);
            }

            order.ShippingCode = dataResponse?.Data?.Order_code;
            order.Expected_delivery_time = dataResponse?.Data?.Expected_delivery_time;

            order.OrderStatus = DeliveryStatusEnum.AwaitingPickup;
            await _orderRepository.UpdateAsync(order);
        }

        public async Task<PagedResponse<OrderDTO>> GetWithOrderStatus(DeliveryStatusEnum statusEnum, PageRequest request)
        {
            int totalOrder;
            IEnumerable<Order> orders;

            int page = request.page, pageSize = request.pageSize;
            string? key = request.search?.ToLower();

            Expression<Func<Order, DateTime?>> sortExpression = e => e.UpdatedAt;

            if(statusEnum == DeliveryStatusEnum.Proccessing)
            {
                sortExpression = e => e.CreatedAt;
            }

            if (string.IsNullOrEmpty(key))
            {
                totalOrder = await _orderRepository.CountAsync(e => e.OrderStatus == statusEnum);
                orders = await _orderRepository.GetPageOrderByDescendingAsync(page, pageSize, e => e.OrderStatus == statusEnum, sortExpression);
            }

            else
            {
                bool isLong = long.TryParse(key, out long idSearch);

                Expression<Func<Order, bool>> expression =
                    e => e.OrderStatus == statusEnum &&
                    (isLong && e.Id.Equals(idSearch) ||
                    e.PaymentMethodName.ToLower().Contains(key));

                totalOrder = await _orderRepository.CountAsync(expression);
                orders = await _orderRepository.GetPageOrderByDescendingAsync(page, pageSize, expression, sortExpression);
            }

            var items = _mapper.Map<IEnumerable<OrderDTO>>(orders);
            return new PagedResponse<OrderDTO>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalOrder
            };
        }

        public async Task<PagedResponse<OrderDTO>> GetWithOrderStatusUser(string userId, DeliveryStatusEnum statusEnum, PageRequest request)
        {
            int totalOrder;
            IEnumerable<Order> orders;

            int page = request.page, pageSize = request.pageSize;
            string? key = request.search?.ToLower();

            Expression<Func<Order, DateTime?>> sortExpression = e => e.UpdatedAt;

            if (statusEnum == DeliveryStatusEnum.Proccessing)
            {
                sortExpression = e => e.CreatedAt;
            }

            if (string.IsNullOrEmpty(key))
            {
                totalOrder = await _orderRepository.CountAsync(e => e.UserId == userId && e.OrderStatus == statusEnum);
                orders = await _orderRepository.GetPageOrderByDescendingAsync(page, pageSize, x => x.UserId == userId && x.OrderStatus == statusEnum, sortExpression);
            }

            else
            {
                bool isLong = long.TryParse(key, out long idSearch);

                Expression<Func<Order, bool>> expression =
                    e => e.OrderStatus == statusEnum &&
                    e.UserId == userId &&
                    (isLong && e.Id.Equals(idSearch) ||
                    e.PaymentMethodName.ToLower().Contains(key));

                totalOrder = await _orderRepository.CountAsync(expression);
                orders = await _orderRepository.GetPageOrderByDescendingAsync(page, pageSize, expression, sortExpression);
            }

            var items = _mapper.Map<IEnumerable<OrderDTO>>(orders);
            foreach (var item in items)
            {
                var p = (await _orderDetailRepository.GetAsync(x => x.OrderId == item.Id)).FirstOrDefault()?.Product;
                item.Product = _mapper.Map<ProductDTO>(p);
            }
            return new PagedResponse<OrderDTO>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalOrder
            };
        }

        public async Task Review(long orderId, string userId, IEnumerable<ReviewRequest> reviews)
        {
            try
            {
                var order = await _orderRepository.SingleOrDefaultAsync(e => e.Id == orderId && e.UserId == userId)
                    ?? throw new InvalidOperationException(ErrorMessage.ORDER_NOT_FOUND);
                if(order.OrderStatus != DeliveryStatusEnum.Received && order.OrderStatus != DeliveryStatusEnum.Finish)
                {
                    throw new InvalidDataException("Bạn chưa thể đánh giá đơn hàng này!");
                }
                List<ProductReview> pReviews = new();
                List<Product> products = new();

                foreach (var review in reviews)
                {
                    var productPath = pathReviewImages + "/" + review.ProductId;
                    List<string>? pathNames = null;

                    if(review.Images != null)
                    {
                        pathNames = new();
                        var imgNames = review.Images.Select(image =>
                        {
                            var name = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                            pathNames.Add(Path.Combine(productPath, name));
                            return name;
                        }).ToList();
                        await _fileStorage.SaveAsync(productPath, review.Images, imgNames);
                    }

                    var product = await _productRepository.FindAsync(review.ProductId);
                    if(product != null)
                    {
                        var currentStar = product.Rating * product.RatingCount;
                        product.Rating = (currentStar + review.Star) / (product.RatingCount + 1);
                        product.RatingCount += 1;

                        products.Add(product);
                        pReviews.Add(new ProductReview
                        {
                            ProductId = review.ProductId,
                            UserId = userId,
                            Star = review.Star,
                            Description = review.Description,
                            ImagesUrls = pathNames,
                        });
                    }
                }
                await _productReviewRepository.AddAsync(pReviews);
                await _productRepository.UpdateAsync(products);
                order.Reviewed = true;
                await _orderRepository.UpdateAsync(order); 
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SalesRespose> GetSaleDate(DateTime from, DateTime to)
        {
            var result = await _orderRepository.GetAsync(e => e.DateReceived >= from && e.DateReceived <= to.AddDays(1) 
                && (e.OrderStatus == DeliveryStatusEnum.Received || e.OrderStatus == DeliveryStatusEnum.Finish));

            var saleList = result.Select(e => new OrderDTO
            {
                Id = e.Id,
                Total = e.Total,
                PaymentMethodName = e.PaymentMethodName,
                DateReceived = e.DateReceived,

            }).ToList();

            var totalExpense = saleList.Sum(e => e.Total);

            return new SalesRespose
            {
                SaleList = saleList,
                Total = totalExpense,
            };
        }

        public async Task<SalesResposeYearMonth> GetSaleYearMonth(int year, int? month)
        {
            var result = await _orderRepository.GetSaleByMonthYear(year, month);

            return new SalesResposeYearMonth
            {
                SaleList = result,
                Total = result.Sum(e => e.Total),
            };
        }
    }

    
}
