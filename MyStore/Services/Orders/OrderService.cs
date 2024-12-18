﻿using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using MyStore.Constant;
using MyStore.DTO;
using MyStore.Enumerations;
using MyStore.Models;
using MyStore.Repository.CartItemRepository;
using MyStore.Repository.OrderRepository;
using MyStore.Repository.ProductRepository;
using MyStore.Repository.TransactionRepository;
using MyStore.Repository.Users;
using MyStore.Request;
using MyStore.Response;
using MyStore.Services.Caching;
using MyStore.Services.Payments;
using MyStore.Services.SendMail;
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
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISendMailService _sendMailService;


        private readonly string pathReviewImages = "assets/images/reviews";


        public OrderService(IOrderRepository orderRepository, IProductRepository productRepository,
            ICartItemRepository cartItemRepository, IPaymentMethodRepository paymentMethodRepository,
            IMapper mapper, IPaymentService paymentService, IOrderDetailRepository orderDetailRepository,
            PayOS payOS, IServiceScopeFactory serviceScopeFactory, ICachingService cache,
            IConfiguration configuration, IFileStorage fileStorage,
            IProductReviewRepository productReviewRepository, ITransactionRepository transactionRepository,
            IUserRepository userRepository, ISendMailService sendMailService)
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
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
            _sendMailService = sendMailService;
        }

        struct OrderCache
        {
            public string Url { get; set; }
            public long OrderId { get; set; }
            public string? payos_IpAddr { get; set; }
            public string? payos_CreateDate { get; set; }
            public string? payos_OrderInfo { get; set; }
        }

        private double CalculateShip(double price) => price >= 400000 ? 0 : price >= 200000 ? 20000 : 40000;

        public async Task<string?> CreateOrder(string userId, OrderRequest request)
        {
            using var transaction = await _transactionRepository.BeginTransactionAsync();
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
                var details = new List<OrderDetail>();

                //var details = cartItems.Select(cartItem =>
                //{
                //    double price = cartItem.Product.Price - (cartItem.Product.Price * (cartItem.Product.Discount / 100));
                //    price *= cartItem.Quantity;
                //    total += price;

                //    cartItem.Product.Sold += cartItem.Quantity;
                //    cartItem.Product.Quantity -= cartItem.Quantity;
                //    listProductUpdate.Add(cartItem.Product);
                //    var imageUrl = cartItem.Product.Images.FirstOrDefault();
                //    return new OrderDetail
                //    {
                //        OrderId = order.Id,
                //        ProductId = cartItem.ProductId,
                //        ProductName = cartItem.Product.Name,
                //        Quantity = cartItem.Quantity,
                //        OriginPrice = cartItem.Product.Price,
                //        Price = price,
                //        ImageUrl = imageUrl != null ? imageUrl.ImageUrl : null,
                //    };
                //}).ToList();

                foreach (var cartItem in cartItems)
                {
                    var product = await _productRepository.SingleAsync(e => e.Id == cartItem.ProductId);

                    //if (product.Quantity == 0)
                    //{
                    //    throw new InvalidDataException(ErrorMessage.SOLD_OUT);
                    //}

                    if (product.Quantity < cartItem.Quantity)
                    {
                        throw new InvalidDataException($"Chỉ còn {product.Quantity} sản phẩm {product.Name} trong kho. Vui lòng mua ít hơn!");
                    }

                    double price = cartItem.Product.Price - (cartItem.Product.Price * (cartItem.Product.Discount / 100));
                    price *= cartItem.Quantity;
                    total += price;

                    cartItem.Product.Sold += cartItem.Quantity;
                    cartItem.Product.Quantity -= cartItem.Quantity;
                    listProductUpdate.Add(cartItem.Product);

                    var imageUrl = cartItem.Product.Images.FirstOrDefault();
                    details.Add(new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        ProductName = cartItem.Product.Name,
                        Quantity = cartItem.Quantity,
                        OriginPrice = cartItem.Product.Price,
                        Price = price,
                        ImageUrl = imageUrl?.ImageUrl,
                    });
                }

                double shipCost = CalculateShip(total);
                order.ShippingCost = shipCost;
                total += shipCost;

                if (total != request.Total)
                {
                    throw new ArgumentException(ErrorMessage.BAD_REQUEST);
                }

                await _orderDetailRepository.AddAsync(details);
                await _productRepository.UpdateAsync(listProductUpdate);
                await _cartItemRepository.DeleteAsync(cartItems);

                string? paymentUrl = null;
                if (method.Name == PaymentMethodEnum.PayOS.ToString())
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

                    paymentUrl = await _paymentService.GetPayOSURL(orders);
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
                }
                await transaction.CommitAsync();
                //await SendMail(order, details);
                return paymentUrl;

            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async void OnPayOSDeadline(object key, object? value, EvictionReason reason, object? state)
        {
            if (value != null)
            {
                using var _scope = _serviceScopeFactory.CreateScope();
                var orderRepository = _scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                var payOS = _scope.ServiceProvider.GetRequiredService<PayOS>();

                var data = (OrderCache)value;

                var paymentInfo = await payOS.getPaymentLinkInformation(data.OrderId);
                if (paymentInfo.status == "PAID")
                {
                    var order = await orderRepository.FindAsync(data.OrderId);
                    if (order != null)
                    {
                        if (paymentInfo.amount == order.Total)
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
                else if (paymentInfo.status != "CANCELLED")
                {
                    await payOS.cancelPaymentLink(data.OrderId);
                }
            }
        }

        public async Task<PagedResponse<OrderDTO>> GetOrderByUserId(string userId, PageRequest page)
        {
            var orders = await _orderRepository.GetPageOrderByDescendingAsync(page.page, page.pageSize, e => e.UserId == userId, x => x.CreatedAt);
            var total = await _orderRepository.CountAsync();

            var items = _mapper.Map<IEnumerable<OrderDTO>>(orders);
            foreach (var item in items)
            {
                var p = (await _orderDetailRepository.GetAsync(x => x.OrderId == item.Id)).FirstOrDefault();
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
                        var orderDetail = await _orderDetailRepository.GetAsync(e => e.OrderId == orderId);
                        await SendEmail(order, orderDetail);
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

                    var orderDetail = await _orderDetailRepository.GetAsync(e => e.OrderId == orderId);

                    var listProductUpdate = new List<Product>();

                    foreach (var detail in orderDetail)
                    {
                        var product = await _productRepository.SingleOrDefaultAsync(e => e.Id == detail.ProductId);
                        if (product != null)
                        {
                            detail.Product.Sold -= detail.Quantity;
                            detail.Product.Quantity += detail.Quantity;
                            listProductUpdate.Add(detail.Product);
                        }
                        else continue;
                    }
                    _cache.Remove("Order " + orderId);
                    await _orderRepository.UpdateAsync(order);
                    await _productRepository.UpdateAsync(listProductUpdate);
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

                    var orderDetail = await _orderDetailRepository.GetAsync(e => e.OrderId == orderId);
                    var listProductUpdate = new List<Product>();


                    foreach (var detail in orderDetail)
                    {
                        var product = await _productRepository.SingleOrDefaultAsync(e => e.Id == detail.ProductId);
                        if (product != null)
                        {
                            detail.Product.Sold -= detail.Quantity;
                            detail.Product.Quantity += detail.Quantity;
                            listProductUpdate.Add(detail.Product);
                        }
                        else continue;
                    }

                    _cache.Remove("Order " + orderId);
                    await _orderRepository.UpdateAsync(order);
                    await _productRepository.UpdateAsync(listProductUpdate);
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
            if (order.OrderStatus != DeliveryStatusEnum.Confirmed)
            {
                throw new InvalidDataException(ErrorMessage.BAD_REQUEST);
            }

            var token = _configuration["GHN:Token"];
            var shopId = _configuration["GHN:ShopId"];
            var url = _configuration["GHN:Url"];

            var receiver = order.Receiver.Split('-').Select(e => e?.Trim()).ToArray();
            var to_name = receiver[0];
            var to_phone = receiver[1];

            if (token == null || shopId == null || url == null || to_name == null || to_phone == null)
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

            Expression<Func<Order, DateTime?>> sortExpression = e => e.CreatedAt;

            if (statusEnum == DeliveryStatusEnum.Proccessing)
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

            Expression<Func<Order, DateTime?>> sortExpression = e => e.CreatedAt;

            if (statusEnum == DeliveryStatusEnum.Proccessing)
            {
                sortExpression = e => e.CreatedAt;
            }

            //if (string.IsNullOrEmpty(key))
            //{
            totalOrder = await _orderRepository.CountAsync(e => e.UserId == userId && e.OrderStatus == statusEnum);
            orders = await _orderRepository.GetPageOrderByDescendingAsync(page, pageSize, x => x.UserId == userId && x.OrderStatus == statusEnum, sortExpression);
            //}

            //else
            //{
            //    bool isLong = long.TryParse(key, out long idSearch);

            //    Expression<Func<Order, bool>> expression =
            //        e => e.OrderStatus == statusEnum &&
            //        e.UserId == userId &&
            //        (isLong && e.Id.Equals(idSearch) ||
            //        e.PaymentMethodName.ToLower().Contains(key));

            //    totalOrder = await _orderRepository.CountAsync(expression);
            //    orders = await _orderRepository.GetPageOrderByDescendingAsync(page, pageSize, expression, sortExpression);
            //}

            var items = _mapper.Map<IEnumerable<OrderDTO>>(orders);
            foreach (var item in items)
            {
                var p = (await _orderDetailRepository.GetAsync(x => x.OrderId == item.Id)).FirstOrDefault();
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
                if (order.OrderStatus != DeliveryStatusEnum.Received && order.OrderStatus != DeliveryStatusEnum.Finish)
                {
                    throw new InvalidDataException("Bạn chưa thể đánh giá đơn hàng này!");
                }
                List<ProductReview> pReviews = new();
                List<Product> products = new();

                foreach (var review in reviews)
                {
                    var productPath = pathReviewImages + "/" + review.ProductId;
                    List<string>? pathNames = null;

                    if (review.Images != null)
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
                    if (product != null)
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
            var result = await _orderRepository.GetAsync(e => e.OrderDate >= from && e.OrderDate <= to.AddDays(1)
                && e.OrderStatus != DeliveryStatusEnum.Canceled);

            var saleList = result.Select(e => new OrderDTO
            {
                Id = e.Id,
                Total = e.Total,
                PaymentMethodName = e.PaymentMethodName,
                OrderDate = e.OrderDate,

            }).ToList();

            var totalExpense = saleList.Sum(e => e.Total);

            return new SalesRespose
            {
                SaleList = saleList,
                Total = totalExpense,
            };
        }

        public async Task<SaleProductReponse> GetProductSaleDate(int productId, DateTime from, DateTime to)
        {
            var result = await _orderRepository.GetStatisticProductSaleByDate(productId, from, to);

            return new SaleProductReponse
            {
                SaleList = result,
                Total = result.Sum(e => e.Total),
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

        public async Task<SaleByProductReponse> GetProductSaleYear(int productId, int year, int? month)
        {
            var result = await _orderRepository.GetStatisticProductSaleByYear(productId, year, month);

            return new SaleByProductReponse
            {
                SaleListProduct = result,
                Total = result.Sum(e => e.Total),
            };
        }

        //public async Task SendEmail(Order order, IEnumerable<OrderDetail> orderDetail)
        //{
        //    var pathEmail = _sendMailService.GetPathOrderConfirm;
        //    var pathProduct = _sendMailService.GetPathProductList;

        //    if (File.Exists(pathEmail) && File.Exists(pathProduct))
        //    {
        //        var user = await _userRepository.SingleAsync(x => x.Id == order.UserId);

        //        string body = File.ReadAllText(pathEmail);
        //        body = body.Replace("{OrderDate}", order.OrderDate.ToString());
        //        body = body.Replace("{UserName}", order.Receiver);
        //        body = body.Replace("{Address}", order.DeliveryAddress);
        //        body = body.Replace("{Method}", order.PaymentMethodName);

        //        string productList = File.ReadAllText(pathProduct);
        //        string listProductBody = "";

        //        foreach (var item in orderDetail)
        //        {
        //            string res = productList.Replace("{PRODUCTNAME}", item.ProductName);
        //            res = res.Replace("{QUANTITY}", item.Quantity.ToString());
        //            listProductBody += res;
        //        }
        //        body = body.Replace("{list_product}", listProductBody);
        //        _ = Task.Run(() => _sendMailService.SendEmailAsync(user.Email!, "Cảm ơn bạn đã đặt hàng tại ZuiZui Shop!", body));
        //    }
        //}

        public async Task SendEmail(Order order, IEnumerable<OrderDetail> orderDetail)
        {
            var pathEmail = _sendMailService.GetPathOrderConfirm;
            var pathProduct = _sendMailService.GetPathProductList;

            if (File.Exists(pathEmail) && File.Exists(pathProduct))
            {
                var user = await _userRepository.SingleAsync(x => x.Id == order.UserId);
                if (user != null)
                {
                    string body = File.ReadAllText(pathEmail);
                    body = body.Replace("{OrderId}", order.Id.ToString());
                    body = body.Replace("{OrderDate}", order.OrderDate.ToString("dd/MM/yyyy"));
                    body = body.Replace("{DateReceived}", order.DateReceived.ToString("dd/MM/yyyy"));
                    body = body.Replace("{UserName}", order.Receiver);
                    body = body.Replace("{TotalPrice}", order.Total.ToString("N0"));
                    body = body.Replace("{ShippingCost}", order.ShippingCost.ToString("N0"));
                    body = body.Replace("{PaymentMethodName}", order.PaymentMethodName);
                    body = body.Replace("{AmountPaid}", order.AmountPaid.ToString("N0"));


                    string productList = File.ReadAllText(pathProduct);
                    string listProductBody = "";

                    foreach (var item in orderDetail)
                    {
                        string res = productList.Replace("{PRODUCTNAME}", item.ProductName)
                                                .Replace("{QUANTITY}", item.Quantity.ToString())
                                                .Replace("{PRICE}", item.Price.ToString("N0"));
                        listProductBody += res;
                    }

                    body = body.Replace("{list_product}", listProductBody);

                    _ = Task.Run(() => _sendMailService.SendEmailAsync(
                        user.Email!,
                        "Cảm ơn bạn đã đặt hàng tại ZuiZui Shop!",
                        body
                    ));
                }


            }
        }

        public async Task<PagedResponse<ProductDTO>> OrderByDescendingBySold(int page, int pageSize)
        {
            int totalProduct;
            IEnumerable<Product> products;
            totalProduct = await _orderDetailRepository.CountSold();
            products = await _orderDetailRepository.OrderByDescendingBySoldInCurrentMonth(page, pageSize);

            //totalProduct = products.Count();

            var res = _mapper.Map<IEnumerable<ProductDTO>>(products)
                .Select(e =>
                {
                    e.ImageUrl = e.Description;
                    return e;
                });
            
            return new PagedResponse<ProductDTO>
            {
                Items = res,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalProduct
            };
        }

    }


}
