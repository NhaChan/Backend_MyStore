using MyStore.DTO;
using MyStore.Models;

namespace MyStore.Response
{
    public class ExpenseReponse
    {
        public IList<StockReceiptDTO>? ExpenseList { get; set; }
        public double Total { get; set; }
    }
    public class ExpenseYearMonthReponse
    {
        public IEnumerable<StatisticData> ExpenseList { get; set; }
        public double Total { get; set; }
    }
}
