using MyStore.Models;
using MyStore.Repository.CommonRepository;
using System.Linq.Expressions;

namespace MyStore.Repository.ProductRepository
{
    public interface IProductReviewRepository : ICommonRepository<ProductReview>
    {
    }
}
