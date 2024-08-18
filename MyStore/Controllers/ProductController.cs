using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.Request;
using MyStore.Services.Products;

namespace MyStore.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService) => _productService = productService;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PageRequest request)
        {
            try
            {
                return Ok(await _productService.GetAllProductAsync(request.page, request.pageSize, request.search));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] ProductRequest request, [FromForm] IFormCollection form)
        {
            try
            {
                var images = form.Files;
                var product = await _productService.CreatedProductAsync(request, images);
                return Ok(product);
            } catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
