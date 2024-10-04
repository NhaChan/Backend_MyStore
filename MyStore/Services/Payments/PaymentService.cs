using AutoMapper;
using MyStore.DTO;
using MyStore.Repository.OrderRepository;

namespace MyStore.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IMapper _mapper;

        public PaymentService(IPaymentMethodRepository paymentMethodRepository, IMapper mapper)
        {
            _paymentMethodRepository = paymentMethodRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PaymentMethodDTO>> GetPaymentMethod()
        {
            var payment = await _paymentMethodRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<PaymentMethodDTO>>(payment);
        }

        public async Task<string?> IsActivePaymentMethod(int id)
        {
            var result = await _paymentMethodRepository.SingleOrDefaultAsync(x => x.Id == id && x.IsActive);
            return result?.Name;
        }
    }
}
