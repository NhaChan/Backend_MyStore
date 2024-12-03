using MyStore.Models;
using MyStore.Repository.CommonRepository;

namespace MyStore.Repository.ImageRepository
{
    public interface IImageRepository : ICommonRepository<Image>
    {
        Task<Image?> GetFirstImageByProductAsync(int id);
        Task<IEnumerable<Image>> GetImageProductAsync(int ProductId);
    }
}
