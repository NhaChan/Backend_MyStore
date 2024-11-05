using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyStore.Models;
using MyStore.Request;
using MyStore.Services.Brands;

namespace MyStore.Controllers
{
    [Route("api/brands")]
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
        public async Task<IActionResult> GetAllBrand()
        {
            try
            {
                //var e = User.FindFirstValue(ClaimTypes.Email);
                var result =await _brandService.GetAllBrandsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        //[HttpGet("{id}")]
        //[Authorize(Roles = "Admin")]
        //public async Task<ActionResult<Brand>> GetBrandById(int id)
        //{
        //    var brand = await _brandService.GetBrandById(id);
        //    if(brand == null)
        //    {
        //        return NotFound();
        //    }
        //    return brand;
        //}

        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateBrand([FromForm] NameRequest request, [FromForm] IFormCollection files)
        {
            try
            {
                var image = files.Files.First();
                var brand = await _brandService.CreateBrandAsync(request.Name, image);
                return Ok(brand);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateBrand(int id, [FromForm] NameRequest request, [FromForm] IFormCollection files)
        {
            try
            {
                var image = files.Files.FirstOrDefault();
                var brand = await _brandService.UpdateBrandAsync(id, request.Name, image);
                return Ok(brand);
            }
            catch(ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            try
            {
                await _brandService.DeleteBrandAsync(id);
                return NoContent();
            }
            catch(ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
