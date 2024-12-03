using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.Services.Expenses;

namespace MyStore.Controllers
{
    [Route("api/expense")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class ExpenseController(IExpenseService expenseService) : Controller
    {
        private readonly IExpenseService _expenseService = expenseService;

        [HttpGet("user")]
        public async Task<IActionResult> GetU()
        {
            try
            {
                var users = await _expenseService.GetCountUser();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("product")]
        public async Task<IActionResult> GetP()
        {
            try
            {
                var products = await _expenseService.GetCountProduct();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("order")]
        public async Task<IActionResult> GetO()
        {
            try
            {
                var orders = await _expenseService.GetCountOrder();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("order-cancel")]
        public async Task<IActionResult> GetOC()
        {
            try
            {
                var orderCancels = await _expenseService.GetOrderCancel();
                return Ok(orderCancels);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        
    }
}
