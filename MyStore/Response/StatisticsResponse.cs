using MyStore.DTO;

namespace MyStore.Response
{
    public class StatisticsResponse
    {
        public ExpenseReponse Expense { get; set; }
        public SalesRespose Sale { get; set; }
        public double Total { get; set; }
    }

    public class StatisticsYearMonthResponse
    {
        public ExpenseYearMonthReponse Expense { get; set; }
        public SalesResposeYearMonth Sale { get; set; }
        public double Total { get; set; }
    }

    public class StatisticData
    {
        public int Time { get; set; }
        public double Total { get; set; }
    }
}
