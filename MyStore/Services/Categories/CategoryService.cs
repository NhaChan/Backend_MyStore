using AutoMapper;
using MyStore.Constant;
using MyStore.DTO;
using MyStore.Models;
using MyStore.Repository.CategoryRepository;
using MyStore.Storage;

namespace MyStore.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IFileStorage _fileStorage;
        private readonly string path = "assets/images/categories";

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper, IFileStorage fileStorage)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _fileStorage = fileStorage;
        }
        public async Task<CategoryDTO> AddCategoryAsync(string name, IFormFile image)
        {
           try
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                Category category = new()
                {
                    Name = name,
                    ImageUrl = Path.Combine(path, fileName),
                };
                await _categoryRepository.AddAsync(category);
                await _fileStorage.SaveAsync(path, image, fileName);
                return _mapper.Map<CategoryDTO>(category);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }

        }

        public async Task DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _categoryRepository.FindAsync(id);
                if (category != null)
                {
                    _fileStorage.Delete(category.ImageUrl);
                    await _categoryRepository.DeleteAsync(id);
                }
                else throw new Exception($"ID {id} " + ErrorMessage.NOT_FOUND);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDTO>>(categories);
        }

        public async Task<CategoryDTO> UpdateCategoryAsync(int id, string name, IFormFile? image)
        {
            var category = await _categoryRepository.FindAsync(id);
            if (category != null)
            {
                category.Name = name;

                if (image != null)
                {
                    _fileStorage.Delete(category.ImageUrl);

                    string fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                    category.ImageUrl = Path.Combine(path, fileName);

                    await _fileStorage.SaveAsync(path, image, fileName);
                }
                
                await _categoryRepository.UpdateAsync(category);
                return _mapper.Map<CategoryDTO>(category);
            }
            else throw new ArgumentException($"ID {id}" + ErrorMessage.NOT_FOUND);
        }
    }
}
