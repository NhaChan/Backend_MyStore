using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyStore.Enumerations;
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
                var url = await _orderService.CreateOrder(userId, request);
                return Ok(url);
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetail(long id)
        {
            try
            {
                var role = User.FindAll(ClaimTypes.Role).Select(e => e.Value);
                var isAdmin = role.Contains("Admin");

                if(isAdmin)
                {
                    var orderDetail = await _orderService.GetOrderDetails(id);
                    return Ok(orderDetail);
                }
                else
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if(userId == null)
                    {
                        return Unauthorized();
                    }
                    var orderDetailUser = await _orderService.GetOrderDetailUser(id, userId);
                    return Ok(orderDetailUser);
                }
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(long id, OrderStatusRequest request)
        {
            try
            {
                await _orderService.UpdateOrderStatus(id, request);
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(long id)
        {
            try
            {
                var role = User.FindAll(ClaimTypes.Role).Select(e => e.Value);
                var isAdmin = role.Contains("Admin");
                if(isAdmin)
                {
                    await _orderService.CancelOrder(id);
                    return Ok();
                }
                else
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (userId == null)
                    {
                        return Unauthorized();
                    }
                    await _orderService.CancelOrder(id, userId);
                    return Ok();
                }

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

        [HttpPut("next-status/{orderId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> NextOrderStatus(long orderId)
        {
            try
            {
                await _orderService.NextOrderStatus(orderId);
                return Ok();
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("shipping/{orderId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Shipping(long orderId, [FromBody] OrderToShippingRequest request)
        {
            try
            {
                await _orderService.OrderToShipping(orderId, request);
                return Ok();
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("status/{status}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetWithOrderStatus(DeliveryStatusEnum status, [FromQuery] PageRequest request)
        {
            try
            {
                var result = await _orderService.GetWithOrderStatus(status, request);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("review/{id}")]
        public async Task<IActionResult> Review(long id, [FromForm] IEnumerable<ReviewRequest> reviews)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }
                await _orderService.Review(id, userId, reviews);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
    