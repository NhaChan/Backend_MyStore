using MyStore.Request;

namespace MyStore.Services.Orders
{
    public interface IOrderService
    {
        Task<string?> CreateOrder(string userId, OrderRequest request);
    }
}
