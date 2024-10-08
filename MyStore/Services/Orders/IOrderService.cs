using MyStore.DTO;
using MyStore.Request;
using MyStore.Response;

namespace MyStore.Services.Orders
{
    public interface IOrderService
    {
        Task<string?> CreateOrder(string userId, OrderRequest request);
        Task<PagedResponse<OrderDTO>> GetOrderByUserId(string userId, PageRequest page);
        Task<PagedResponse<OrderDTO>> GetAllOrder(int page, int pageSize, string? search);
        Task<OrderDetailsResponse> GetOrderDetails(int id);
    }
}
