using MyStore.DTO;
using MyStore.Models;
using MyStore.Request;

namespace MyStore.Services.Brands
{
    public interface IBrandService
    {
        Task<IEnumerable<BrandDTO>> GetAllBrandsAsync();
        //Task<BrandDTO> GetBrandById(int id);
        Task<BrandDTO> CreateBrandAsync(string Name, IFormFile image);
        Task<BrandDTO> UpdateBrandAsync(int id, string Name, IFormFile? image);
        Task DeleteBrandAsync(int id);
    }
}
