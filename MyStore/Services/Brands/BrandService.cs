using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using MyStore.Data;
using MyStore.Models;
using MyStore.Request;
using MyStore.Response;

namespace MyStore.Services.Brands
{
    public class BrandService : IBrandService
    {
        private readonly CompanyDBContext _context;
        public BrandService(CompanyDBContext context)
        {
            _context = context;
        }

        public async Task<Brand> CreateBrand(BrandRequest request)
        {
            var filename = Guid.NewGuid().ToString() + Path.GetExtension(request.Image.FileName);
            var newbrand = new Brand()
            {
                BrandName = request.BrandName,
                Picture = filename,
            };

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", filename);
            using (var stream = File.Create(path))
            {
                await request.Image.CopyToAsync(stream);
            }


            await _context.Brands.AddAsync(newbrand);
            await _context.SaveChangesAsync();

            return newbrand;
        }

        public async Task<IList<Brand>> GetAllBrands(string? search)
        {
            var allproduct = _context.Brands.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                allproduct = allproduct.Where(e => e.BrandName.Contains(search));
            }
            var result = await allproduct.Select(e => new Brand()
            {
                BrandName = e.BrandName,
                Picture = e.Picture,
            }).ToListAsync();

            return result;

            //return await _context.Brands.ToListAsync();
        }

        public async Task<Brand> GetBrandById(long id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                throw new KeyNotFoundException($"Brand with ID {id} not found.");
            }
            return brand;
        }

        public async Task<Brand> UpdateBrand(long id, Brand brand)
        {
            var updateBrand = await _context.Brands.FindAsync(id);
            if (updateBrand == null)
            {
                throw new KeyNotFoundException($"Brand with {id} not found!");
            };
            updateBrand.BrandName = brand.BrandName;

            _context.Brands.Update(updateBrand);

            _context.SaveChanges();

            return updateBrand;
        }
        public async Task<Brand> DeleteBrand(long id)
        {
            var dbrand = await _context.Brands.FindAsync(id);
            if (dbrand == null)
            {
                throw new KeyNotFoundException($"Brand with {id} not found!");
            }
            _context.Brands.Remove(dbrand);
            _context.SaveChanges();

            return dbrand;
        }
    }
}
