using MyStore.Response;

namespace MyStore.Services.Statistics
{
    public interface IStatisticsService
    {
        Task<StatisticsResponse> GetStatistics(DateTime from, DateTime to);
        Task<StatisticsYearMonthResponse> GetStatisticsByYearMonth(int year, int? month);
    }
}
