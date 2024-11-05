using MyStore.Response;

namespace MyStore.Services.Statistics
{
    public interface IStatisticsService
    {
        Task<StatisticsResponse> GetStatistics(DateTime from, DateTime to);
        Task<StatisticsYearMonthResponse> GetStatisticsByYearMonth(int year, int? month);
        Task<StatisticProductResponse> GetProductStatisticsByYear(int productId, int year, int? month);
        Task<StatisticProductResponse> GetStatisticsProductByDate(int productId, DateTime from, DateTime to);
    }
}
