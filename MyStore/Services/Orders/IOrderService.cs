using MyStore.DTO;
using MyStore.Enumerations;
using MyStore.Request;
using MyStore.Response;

namespace MyStore.Services.Orders
{
    public interface IOrderService
    {
        Task<string?> CreateOrder(string userId, OrderRequest request);
        Task<PagedResponse<OrderDTO>> GetOrderByUserId(string userId, PageRequest page);
        Task<PagedResponse<OrderDTO>> GetAllOrder(int page, int pageSize, string? search);
        Task<OrderDetailsResponse> GetOrderDetails(long orderId);
        Task<OrderDetailsResponse> GetOrderDetailUser(long orderId, string userId);
        Task UpdateOrderStatus(long orderId, OrderStatusRequest reques);
        Task CancelOrder(long orderId);
        Task CancelOrder(long orderId, string userId);
        Task NextOrderStatus(long orderId);
        Task OrderToShipping(long orderId, OrderToShippingRequest request);
        Task<PagedResponse<OrderDTO>> GetWithOrderStatus(DeliveryStatusEnum statusEnumm, PageRequest request);
        Task Review(long orderId, string userId, IEnumerable<ReviewRequest> reviews);
    }
}
