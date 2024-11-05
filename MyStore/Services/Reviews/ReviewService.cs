using AutoMapper;
using MyStore.Constant;
using MyStore.DTO;
using MyStore.Repository.ProductRepository;
using MyStore.Request;
using MyStore.Response;
using MyStore.Storage;

namespace MyStore.Services.Reviews
{
    public class ReviewService : IReviewService
    {
        private readonly IProductReviewRepository _productReviewRepository;
        private readonly IMapper _mapper;
        private readonly IFileStorage _fileStorage;
        private readonly string path = "assets/images/reviews";
        private readonly IProductRepository _productRepository;

        public ReviewService(IProductReviewRepository productReviewRepository, IMapper mapper, IFileStorage fileStorage, IProductRepository productRepository)
        {
            _productReviewRepository = productReviewRepository;
            _mapper = mapper;
            _fileStorage = fileStorage;
            _productRepository = productRepository;
        }

        public async Task Delete(string id)
        {
            try
            {
                var review = await _productReviewRepository.FindAsync(id);
                if (review != null)
                {
                    var product = await _productRepository.FindAsync(review.ProductId);

                    if (product != null)
                    {
                        var currentStar = product.Rating * product.RatingCount;
                        product.Rating = (currentStar - review.Star) / (product.RatingCount - 1);
                        product.RatingCount -= 1;

                        await _productRepository.UpdateAsync(product);

                    }
                    _fileStorage.Delete(review.ImagesUrls ?? []);
                    await _productReviewRepository.DeleteAsync(id);

                }
                else throw new Exception($"ID {id} " + ErrorMessage.NOT_FOUND);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        //public async Task<PagedResponse<ReviewDTO>> GetAllReview(PageRequest request)
        //{
        //    var reviews = await _productReviewRepository.GetPageOrderByDescendingAsync(request.page, request.pageSize, null, e => e.CreatedAt);
        //    if (reviews == null)
        //    {
        //        throw new Exception("Lỗi");
        //    }

        //    var total = await _productReviewRepository.CountAsync();

        //    var items = _mapper.Map<IEnumerable<ReviewDTO>>(reviews);

        //    return new PagedResponse<ReviewDTO>
        //    {
        //        Items = items,
        //        TotalItems = total,
        //        Page = request.page,
        //        PageSize = request.pageSize
        //    };
        //}
    }
}
