using AutoMapper;
using MyStore.Constant;
using MyStore.DTO;
using MyStore.Enumerations;
using MyStore.Models;
using MyStore.Repository.OrderRepository;
using MyStore.Services.Caching;
using Net.payOS;
using Net.payOS.Types;

namespace MyStore.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly PayOS _payOS;
        private readonly IOrderRepository _orderRepository;
        private readonly ICachingService _cache;

        public PaymentService(IPaymentMethodRepository paymentMethodRepository, IMapper mapper,
            IConfiguration configuration, PayOS payOS, IOrderRepository orderRepository,
            ICachingService cache)
        {
            _paymentMethodRepository = paymentMethodRepository;
            _payOS = payOS;
            _mapper = mapper;
            _configuration = configuration;
            _orderRepository = orderRepository;
            _cache = cache;
        }

        public async Task<IEnumerable<PaymentMethodDTO>> GetPaymentMethod()
        {
            var payment = await _paymentMethodRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<PaymentMethodDTO>>(payment);
        }

        public async Task<string> GetPayOSURL(PayOSOrderInfo orderInfo)
        {
            var cancelUrl = _configuration["PayOS:cancelUrl"];
            var returnUrl = _configuration["PayOS:returnUrl"];

            if(cancelUrl == null || returnUrl == null)
            {
                throw new ArgumentNullException(ErrorMessage.ARGUMENT_NULL);
            }

            List<ItemData> items = orderInfo.Products
                .Select(e => new ItemData("Mi", e.Quantity, (int)Math.Floor(e.Price))).ToList();
            PaymentData paymentData = new(orderInfo.OrderId, (int)Math.Floor(orderInfo.Amount),
                "Thanh toan don hang: " + orderInfo.OrderId, items, cancelUrl, returnUrl);

            CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);
            return createPayment.checkoutUrl;
        }

        public async Task<string?> IsActivePaymentMethod(int id)
        {
            var result = await _paymentMethodRepository.SingleOrDefaultAsync(x => x.Id == id && x.IsActive);
            return result?.Name;
        }

        public async Task PayOSCallback(PayOSRequest request)
        {
            if(request.Code == "00")
            {
                var order = await _orderRepository.FindAsync(request.Code);
                if(order != null)
                {
                    var paymentInfo = await _payOS.getPaymentLinkInformation(request.OrderCode);
                    if(request.Cancel || request.Status == "CANCELLED")
                    {
                        _cache.Remove("Order " + request.OrderCode);
                        order.OrderStatus = DeliveryStatusEnum.Canceled;
                        await _orderRepository.UpdateAsync(order);
                        throw new InvalidDataException(ErrorMessage.PAYMENT_FAILED);
                    }
                    else
                    {
                        if (paymentInfo.amount == order.Total)
                        {
                            if (paymentInfo.status == "PAID")
                            {
                                order.PaymentTranId = request.Id;
                                order.AmountPaid = paymentInfo.amountPaid;
                                order.OrderStatus = DeliveryStatusEnum.Confirmed;
                            }
                            await _orderRepository.UpdateAsync(order);
                        }
                        else throw new ArgumentException("Số tiền " + ErrorMessage.INVALID);
                    }
                }
                else throw new InvalidOperationException(ErrorMessage.NOT_FOUND + " đơn hàng");
            }
            else
                {
                    _cache.Remove("Order " + request.OrderCode);
                    throw new Exception(ErrorMessage.ERROR);
                }
        }
    }
}
