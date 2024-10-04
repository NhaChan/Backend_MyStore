using MyStore.DTO;

namespace MyStore.Services.Payments
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentMethodDTO>> GetPaymentMethod();
        Task<string?> IsActivePaymentMethod(int id);
    }
}
