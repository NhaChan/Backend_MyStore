using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.Request;
using MyStore.Services.Reviews;

namespace MyStore.Controllers
{
    [Route("api/review")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class ReviewController(IReviewService reviewService) : ControllerBase
    {
        private readonly IReviewService _reviewService = reviewService;

        //[HttpGet]
        //public async Task<IActionResult> GetAll([FromQuery] PageRequest request)
        //{
        //    try
        //    {
        //        var result = await _reviewService.GetAllReview(request);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //}

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _reviewService.Delete(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }



}
