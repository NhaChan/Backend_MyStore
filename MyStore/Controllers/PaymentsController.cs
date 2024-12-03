using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.Models;
using MyStore.Services.Payments;

namespace MyStore.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class PaymentsController(IPaymentService paymentService) : ControllerBase
    {
        private readonly IPaymentService _paymentService = paymentService;

        [HttpGet]
        public async Task<IActionResult> GetPaymentMethods()
        {
            try
            {
                var result = await _paymentService.GetPaymentMethod();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("payos-callback")]
        public async Task<IActionResult> PayOSCallback([FromQuery] PayOSRequest request)
        {
            try
            {
                await _paymentService.PayOSCallback(request);
                return NoContent();

            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
