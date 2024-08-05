using Microsoft.AspNetCore.Mvc;
using MyStore.Models;
using MyStore.Request;
using MyStore.Services.Brands;

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
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllBrands(string? search)
        {
            try
            {
                //var e = User.FindFirstValue(ClaimTypes.Email);
                var result =await _brandService.GetAllBrands(search);
                return Ok(result);
            }
            catch
            {
                return BadRequest("We can not get the products");
            }
        }

        [HttpGet("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<Brand>> GetBrandById(long id)
        {
            var brand = await _brandService.GetBrandById(id);
            if(brand == null)
            {
                return NotFound();
            }
            return brand;
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<Brand>> CreateBrand([FromForm] BrandRequest request)
        {
            if(request == null)
            {
                return BadRequest();
            }
            var createBrand = await _brandService.CreateBrand(request);
            return CreatedAtAction(nameof(GetBrandById), new { id = createBrand.BrandID }, createBrand);
        }
        [HttpPut("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<Brand>> UpdateBrand(long id, Brand brand)
        {
            if(brand == null)
            {
                return BadRequest();
            }
            try
            {
                var updateBrand = await _brandService.UpdateBrand(id, brand);
                return Ok(updateBrand);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBrand(long id)
        {
            var dbrand = await _brandService.DeleteBrand(id);
            if(dbrand == null)
            {
                return NotFound();
            }
            return NoContent();
        }

        
    }
}
