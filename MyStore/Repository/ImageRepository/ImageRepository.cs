using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using MyStore.Repository.CommonRepository;

namespace MyStore.Repository.ImageRepository
{
    public class ImageRepository : CommonRepository<Image>, IImageRepository
    {
        private readonly CompanyDBContext _dbContext;
        public ImageRepository(CompanyDBContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Image?> GetFirstImageByProductAsync(int id)
        {
            return await _dbContext.Images.FirstOrDefaultAsync(e => e.ProductId == id);
        }

        public async Task<IEnumerable<Image>> GetImageProductAsync(int ProductId)
        {
            return await _dbContext.Images.Where(e => e.ProductId == ProductId).ToListAsync();
        }
    }
}
