using MyStore.DTO;

namespace MyStore.Response
{
    public class SalesRespose
    {
        public IList<OrderDTO>? SaleList { get; set; }
        public double Total { get; set; }
    }

    public class SalesResposeYearMonth
    {
        public IEnumerable<StatisticData> SaleList { get; set; }
        public double Total { get; set; }
    }
}
