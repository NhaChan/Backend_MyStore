﻿using MyStore.DTO;
using MyStore.Repository.OrderRepository;
using MyStore.Repository.StockReceiptRepository;
using MyStore.Response;
using MyStore.Services.Expenses;
using MyStore.Services.Orders;
using MyStore.Services.StockReceipts;

namespace MyStore.Services.Statistics
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IStockReceiptService _stockReceiptService;
        private readonly IOrderService _orderService;

        public StatisticsService(IStockReceiptService stockReceiptService, IOrderService orderService)
        {
            _stockReceiptService = stockReceiptService;
            _orderService = orderService;
        }


        public async Task<StatisticsResponse> GetStatistics(DateTime from, DateTime to)
        {
            ExpenseReponse expense = await _stockReceiptService.GetExpenseDate(from, to);
            SalesRespose sale = await _orderService.GetSaleDate(from, to);

            var SumTotal = sale.Total - expense.Total;
            return new StatisticsResponse { Expense = expense, Sale = sale, Total = SumTotal };
        }

        public async Task<StatisticsYearMonthResponse> GetStatisticsByYearMonth(int year, int? month)
        {
            ExpenseYearMonthReponse expense = await _stockReceiptService.GetExpenseYearMonth(year, month);
            SalesResposeYearMonth sale = await _orderService.GetSaleYearMonth(year, month);

            var SumTotal = sale.Total - expense.Total;
            return new StatisticsYearMonthResponse { Expense = expense, Sale = sale, Total = SumTotal };
        }


    }
}