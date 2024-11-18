using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.Request;
using MyStore.Services.LogHistory;

namespace MyStore.Controllers
{
    [Route("api/log")]
    [ApiController]
    public class LogController(ILogService logService) : ControllerBase
    {
        private readonly ILogService _logService = logService;

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Get([FromQuery] PageRequest request) 
        {
            try
            {
                var result = await _logService.GetAllLog(request.page, request.pageSize, request.search);
                return Ok(result);
            } 
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetId(long id)
        {
            try
            {
                var result = await _logService.GetLogDetails(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
