namespace MyStore.Response
{
    public class ProductStatisticResponse
    {
        public ExpenseProductReponse Expense { get; set; }
        public SaleProductReponse Sale { get; set; }
        public double Total { get; set; }
    }

    public class StatisticProduct
    {
        public DateTime Time { get; set; }
        public double Total { get; set; }
    }

    public class ExpenseProductReponse
    {
        public IEnumerable<StatisticProduct> ExpenseList { get; set; }
        public double Total { get; set; }
    }

    public class SaleProductReponse
    {
        public IEnumerable<StatisticProduct> SaleList { get; set; }
        public double Total { get; set; }
    }
}
