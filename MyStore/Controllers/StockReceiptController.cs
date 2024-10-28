using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyStore.Request;
using MyStore.Services.StockReceipts;
using System.Security.Claims;

namespace MyStore.Controllers
{
    [Route("api/stock")]
    [ApiController]
    public class StockReceiptController(IStockReceiptService stockReceiptService) : ControllerBase
    {
        private readonly IStockReceiptService _stockReceiptService = stockReceiptService;

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] StockReceiptRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if(userId == null)
                {
                    return Unauthorized();
                }
                var result = await _stockReceiptService.CreateStockReceipt(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] PageRequest request)
        {
            var result = await _stockReceiptService.GetAllStock(request.page, request.pageSize, request.search);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStockDetail(long id)
        {
            try
            {
                var stockDetail = await _stockReceiptService.GetStockDetails(id);
                return Ok(stockDetail);
            }
            catch(InvalidOperationException ex)
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
