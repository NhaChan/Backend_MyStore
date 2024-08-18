using MyStore.DTO;
using MyStore.Request;
using MyStore.Response;

namespace MyStore.Services.Products
{
    public interface IProductService
    {
        Task <PagedResponse<ProductDTO>> GetAllProductAsync(int page, int pageSize, string? search);
        Task<ProductDTO> CreatedProductAsync(ProductRequest request, IFormFileCollection images);
    }
}
