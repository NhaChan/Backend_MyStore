using Microsoft.AspNetCore.Authorization;
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
        //[Authorize(Roles = "Admin")]
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

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _productService.GetProductById(id);
                return Ok(result);
            } catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            } catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("filters")]
        public async Task<IActionResult> GetFilterProducts([FromQuery] Filters filters)
        {
            try
            {
                var result = await _productService.GetFilterProductsAsync(filters);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
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

        [HttpPut("update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] ProductRequest request, [FromForm] IFormCollection form)
        {
            try
            {
                var images = form.Files;
                var result = await _productService.UpdateProduct(id, request, images);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message) ;
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("updateEnable/{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProductEnable(int id, [FromBody] UpdateEnableRequest request)
        {
            try
            {
                var result = await _productService.UpdateProductEnableAsync(id, request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("name")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetNameProduct()
        {
            try
            {
                var namrProduct = await _productService.GetNameProduct();
                return Ok(namrProduct);
            }
            catch(Exception ex) { return StatusCode(500, ex.Message);}
        }

        [HttpGet("{id}/reviews")]
        public async Task<IActionResult> GetReviews(int id, [FromQuery] PageRequest request)
        {
            try
            {
                var result = await _productService.GetReviews(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}