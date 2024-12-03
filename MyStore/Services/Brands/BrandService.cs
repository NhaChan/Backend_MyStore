using AutoMapper;
using MyStore.DTO;
using MyStore.Constant;
using MyStore.Models;
using MyStore.Repository.BrandRepository;
using MyStore.Storage;

namespace MyStore.Services.Brands
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IMapper _mapper;
        private readonly IFileStorage _fileStorage;
        private readonly string path = "assets/images/brands";

        public BrandService(IBrandRepository brandRepository, IMapper mapper, IFileStorage fileStorage)
        {
            _brandRepository = brandRepository;
            _mapper = mapper;
            _fileStorage = fileStorage;
        }
        public async Task<BrandDTO> CreateBrandAsync(string Name, IFormFile image)
        {
            try
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

                Brand brand = new()
                {
                    Name = Name,
                    ImageUrl = Path.Combine(path, fileName),
                };

                await _brandRepository.AddAsync(brand);
                await _fileStorage.SaveAsync(path, image, fileName);
                return _mapper.Map<BrandDTO>(brand);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task DeleteBrandAsync(int id)
        {
            try
            {
                var brand = await _brandRepository.FindAsync(id);
                if (brand != null)
                {
                    _fileStorage.Delete(brand.ImageUrl);
                    await _brandRepository.DeleteAsync(id);
                }
                else throw new Exception($"ID {id} " + ErrorMessage.NOT_FOUND);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task<IEnumerable<BrandDTO>> GetAllBrandsAsync()
        {
            var brands = await _brandRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<BrandDTO>>(brands);
        }

        public async Task<BrandDTO> UpdateBrandAsync(int id, string Name, IFormFile? image)
        {
            var brand = await _brandRepository.FindAsync(id);
            if (brand != null)
            {
                brand.Name = Name;

                if (image != null)
                {
                    _fileStorage.Delete(brand.ImageUrl);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    brand.ImageUrl = Path.Combine(path, fileName);

                    await _fileStorage.SaveAsync(path, image, fileName);
                }
                await _brandRepository.UpdateAsync(brand);

                return _mapper.Map<BrandDTO>(brand);
                       
            }
            else throw new ArgumentException(ErrorMessage.NOT_FOUND);
        }
    }
}


    //    private readonly CompanyDBContext _context;
    //    public BrandService(CompanyDBContext context)
    //    {
    //        _context = context;
    //    }

    //    public async Task<Brand> CreateBrand(BrandRequest request)
    //    {
    //        var filename = Guid.NewGuid().ToString() + Path.GetExtension(request.Image.FileName);
    //        var newbrand = new Brand()
    //        {
    //            Name = request.Name,
    //            Picture = filename,
    //        };

    //        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", filename);
    //        using (var stream = File.Create(path))
    //        {
    //            await request.Image.CopyToAsync(stream);
    //        }


    //        await _context.Brands.AddAsync(newbrand);
    //        await _context.SaveChangesAsync();

    //        return newbrand;
    //    }

    //    public async Task<IList<Brand>> GetAllBrands(string? search)
    //    {
    //        var allproduct = _context.Brands.AsQueryable();
    //        if (!string.IsNullOrEmpty(search))
    //        {
    //            allproduct = allproduct.Where(e => e.Name.Contains(search));
    //        }
    //        var result = await allproduct.Select(e => new Brand()
    //        {
    //            Name = e.Name,
    //            Picture = e.Picture,
    //        }).ToListAsync();

    //        return result;

    //        //return await _context.Brands.ToListAsync();
    //    }

    //    public async Task<Brand> GetBrandById(int id)
    //    {
    //        var brand = await _context.Brands.FindAsync(id);
    //        if (brand == null)
    //        {
    //            throw new KeyNotFoundException($"Brand with ID {id} not found.");
    //        }
    //        return brand;
    //    }

    //    public async Task<Brand> UpdateBrand(int id, Brand brand)
    //    {
    //        var updateBrand = await _context.Brands.FindAsync(id);
    //        if (updateBrand == null)
    //        {
    //            throw new KeyNotFoundException($"Brand with {id} not found!");
    //        };
    //        updateBrand.Name = brand.Name;

    //        _context.Brands.Update(updateBrand);

    //        _context.SaveChanges();

    //        return updateBrand;
    //    }
    //    public async Task<Brand> DeleteBrand(int id)
    //    {
    //        var dbrand = await _context.Brands.FindAsync(id);
    //        if (dbrand == null)
    //        {
    //            throw new KeyNotFoundException($"Brand with {id} not found!");
    //        }
    //        _context.Brands.Remove(dbrand);
    //        _context.SaveChanges();

    //        return dbrand;
    //    }
    //}

