using MyStore.DTO;

namespace MyStore.Services.Categories
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync();
        Task<CategoryDTO> AddCategoryAsync(string name);
        Task<CategoryDTO> UpdateCategoryAsync(int id, string name);
        Task DeleteCategoryAsync(int id);
    }
}
