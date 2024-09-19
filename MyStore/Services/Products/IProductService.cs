using MyStore.DTO;
using MyStore.Models;
using MyStore.Request;
using MyStore.Response;

namespace MyStore.Services.Products
{
    public interface IProductService
    {
        Task <PagedResponse<ProductDTO>> GetAllProductAsync(int page, int pageSize, string? search);
        Task<ProductDTO> CreatedProductAsync(ProductRequest request, IFormFileCollection images);
        Task<ProductDetailResponse> GetProductById(int id);
        Task<ProductDTO> UpdateProduct(int id, ProductRequest productRequest, IFormFileCollection images);
        Task<bool> UpdateProductEnableAsync(int id, UpdateEnableRequest request);
        Task DeleteProductAsync(int id);
        Task<PagedResponse<ProductDTO>> GetFilterProductsAsync(Filters filters);
    }
}
