using MyStore.Models;
using MyStore.Request;

namespace MyStore.Services.Brands
{
    public interface IBrandService
    {
        Task<IList<Brand>> GetAllBrands(string? search);
        Task<Brand> GetBrandById(long id);
        Task<Brand> CreateBrand(BrandRequest request);
        Task<Brand> UpdateBrand(long id, Brand brand);
        Task<Brand> DeleteBrand(long id);
    }
}
