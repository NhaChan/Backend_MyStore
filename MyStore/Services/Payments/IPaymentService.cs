using MyStore.DTO;
using MyStore.Models;

namespace MyStore.Services.Payments
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentMethodDTO>> GetPaymentMethod();
        Task<string?> IsActivePaymentMethod(int id);
        Task<string> GetPayOSURL(PayOSOrderInfo orderInfo);
        Task PayOSCallback(PayOSRequest request);
    }
}
