using AutoMapper;
using MyStore.Constant;
using MyStore.DTO;
using MyStore.Models;
using MyStore.Repository.CategoryRepository;

namespace MyStore.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }
        public async Task<CategoryDTO> AddCategoryAsync(string name)
        {
            var category = new Category
            {
                Name = name
            };
            await _categoryRepository.AddAsync(category);
            return _mapper.Map<CategoryDTO>(category);

        }

        public async Task DeleteCategoryAsync(int id)
        {
            await _categoryRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDTO>>(categories);
        }

        public async Task<CategoryDTO> UpdateCategoryAsync(int id, string name)
        {
            var category = await _categoryRepository.FindAsync(id);
            if (category == null)
            {
                throw new ArgumentException($"ID {id}" + ErrorMessage.NOT_FOUND);
            }
            else
            {
                category.Name = name;
                await _categoryRepository.UpdateAsync(category);
                return _mapper.Map<CategoryDTO>(category);
            }
        }
    }
}
