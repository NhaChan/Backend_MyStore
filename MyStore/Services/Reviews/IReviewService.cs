using MyStore.DTO;
using MyStore.Request;
using MyStore.Response;

namespace MyStore.Services.Reviews
{
    public interface IReviewService
    {
        //Task<PagedResponse<ReviewDTO>> GetAllReview(PageRequest request);
        Task Delete(string id);
    }
}
