﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.Services.Statistics;

namespace MyStore.Controllers
{
    [Route("api/statistic")]
    [ApiController]
    public class StatisticsController(IStatisticsService statisticsService) : ControllerBase
    {
        private readonly IStatisticsService _statisticsService = statisticsService;

        [HttpGet("getStatisticsFormTo")]
        public async Task<IActionResult> Get([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            try
            {
                var result = await _statisticsService.GetStatistics(from, to);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("byYear")]
        public async Task<IActionResult> GetByYear(int year, int? month)
        {
            try
            {
                var result = await _statisticsService.GetStatisticsByYearMonth(year, month);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}