using MyStore.DTO;

namespace MyStore.Services.Categories
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync();
        Task<CategoryDTO> AddCategoryAsync(string name, IFormFile image);
        Task<CategoryDTO> UpdateCategoryAsync(int id, string name, IFormFile? image);
        Task DeleteCategoryAsync(int id);
    }
}
