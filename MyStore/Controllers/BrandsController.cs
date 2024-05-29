using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.Data;
using MyStore.Models;
using MyStore.Services;

namespace MyStore.Controllers
{
    [Route("api/brand")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly IBrandService _brandService;
        public BrandsController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult<List<Brand>> GetAllBrands()
        {
            return _brandService.GetAllBrands();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public ActionResult<Brand> GetBrandById(int id)
        {
            var brand = _brandService.GetBrandById(id);
            if(brand == null)
            {
                return NotFound();
            }
            return brand;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult<Brand> CreateBrand(Brand brand)
        {
            if(brand == null)
            {
                return BadRequest();
            }
            var createBrand = _brandService.CreateBrand(brand);
            return CreatedAtAction(nameof(GetBrandById), new { id = createBrand.BrandID }, createBrand);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public ActionResult<Brand> UpdateBrand(int id, Brand brand)
        {
            if(brand == null)
            {
                return BadRequest();
            }
            try
            {
                var updateBrand = _brandService.UpdateBrand(id, brand);
                return Ok(updateBrand);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
        
    }
}
