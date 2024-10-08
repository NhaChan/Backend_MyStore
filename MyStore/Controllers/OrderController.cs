using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyStore.Request;
using MyStore.Services.Orders;
using System.Security.Claims;

namespace MyStore.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Authorize]
    public class OrderController(IOrderService orderService) : ControllerBase
    {
        private readonly IOrderService _orderService = orderService;

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }
                var order = await _orderService.CreateOrder(userId, request);
                return Ok(order);
            }
            catch(ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("order-user")]
        public async Task<IActionResult> GetOrderByUserId([FromQuery] PageRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if(userId == null)
                {
                    return Unauthorized();
                }
                var orders = await _orderService.GetOrderByUserId(userId, request);
                return Ok(orders);
            } catch(ArgumentException ex)
            {
                return NotFound(ex.Message);
            } catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("get-all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] PageRequest request)
        {
            var result = await _orderService.GetAllOrder(request.page, request.pageSize, request.search);
            return Ok(result);
        }
        
    }
}
    