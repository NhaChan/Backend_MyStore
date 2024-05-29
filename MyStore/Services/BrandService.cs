using MyStore.Data;
using MyStore.Models;

namespace MyStore.Services
{
    public class BrandService : IBrandService
    {
        private readonly CompanyDBContext _context;
        public BrandService(CompanyDBContext context)
        {
            _context = context;
        }

        public Brand CreateBrand(Brand brand)
        {
            var newbrand = new Brand()
            {
                BrandName = brand.BrandName,
            };
            _context.Brands.Add(newbrand); 
            _context.SaveChanges();

            return newbrand;
        }

        public List<Brand> GetAllBrands()
        {
            return _context.Brands.ToList();
        }

        public Brand GetBrandById(int id)
        {
            var brand = _context.Brands.FirstOrDefault(e => e.BrandID == id);
            if (brand == null)
            {
                // Optionally handle the case where the brand is not found
                throw new KeyNotFoundException($"Brand with ID {id} not found.");
            }
            return brand;
        }

        public Brand UpdateBrand(int id, Brand brand)
        {
            var updateBrand = _context.Brands.FirstOrDefault(b => b.BrandID == id);
            if (updateBrand == null)
            {
                throw new KeyNotFoundException($"Brand with {id} not found!");
            };
            updateBrand.BrandName = brand.BrandName;

            _context.Brands.Update(updateBrand);

            _context.SaveChanges() ;

            return updateBrand;
        }
    }
}
