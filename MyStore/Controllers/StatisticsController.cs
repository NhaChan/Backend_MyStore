﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        //[Authorize(Roles = "Admin,Statist")]
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
        [Authorize(Roles = "Admin,Statist,CSKH")]
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

        [HttpGet("by-productYear")]
        [Authorize(Roles = "Admin,Statist")]
        public async Task<IActionResult> GetByProductYear(int productId, int year, int? month)
        {
            try
            {
                var result = await _statisticsService.GetProductStatisticsByYear(productId, year, month);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getStatisticsProductFormTo")]
        [Authorize(Roles = "Admin,Statist")]
        public async Task<IActionResult> GetProduct(int productId, [FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            try
            {
                var result = await _statisticsService.GetStatisticsProductByDate(productId, from, to);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
