using MyStore.Models;

namespace MyStore.Services
{
    public interface IBrandService
    {
        List<Brand> GetAllBrands();
        Brand GetBrandById(int id);
        Brand CreateBrand(Brand brand);
        Brand UpdateBrand(int id, Brand brand);
    }
}
