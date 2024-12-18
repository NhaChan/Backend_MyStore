﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.Request;
using MyStore.Services.Categories;

namespace MyStore.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService) => _categoryService = categoryService;

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var category = await _categoryService.GetAllCategoriesAsync();
                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin,CSKH")]
        public async Task<IActionResult> Creat([FromForm] NameRequest request, [FromForm] IFormCollection files)
        {
            try
            {
                var image = files.Files.First();
                var category = await _categoryService.AddCategoryAsync(request.Name, image);
                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("update/{id}")]
        [Authorize(Roles = "Admin,CSKH")]
        public async Task<IActionResult> Update(int id, [FromForm] NameRequest request, [FromForm] IFormCollection files)
        {
            try
            {
                var image = files.Files.FirstOrDefault();
                var result = await _categoryService.UpdateCategoryAsync(id, request.Name, image);
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
        [Authorize(Roles = "Admin,CSKH")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
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
    }
}
