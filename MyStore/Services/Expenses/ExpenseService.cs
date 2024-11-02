
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using MyStore.Enumerations;
using MyStore.Models;
using MyStore.Repository.OrderRepository;
using MyStore.Repository.ProductRepository;
using MyStore.Repository.StockReceiptRepository;
using MyStore.Repository.Users;
using MyStore.Response;

namespace MyStore.Services.Expenses
{
    public class ExpenseService : IExpenseService
    {
        private readonly IUserRepository _userRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IStockReceiptRepository _stockReceiptRepository;
        
        public ExpenseService(IUserRepository userRepository, IProductRepository productRepository,
            IOrderRepository orderRepository, IStockReceiptRepository stockReceiptRepository)
        {
            _userRepository = userRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _stockReceiptRepository = stockReceiptRepository;
        }
        public async Task<int> GetCountUser()
        {
            var users = await _userRepository.CountAsync();
            return users;
        }

        public async Task<int> GetCountProduct()
        {
            var products = await _productRepository.CountAsync();
            return products;
        }

        public async Task<int> GetCountOrder()
        {
            var order1 = await _orderRepository.CountAsync(e => e.OrderStatus == DeliveryStatusEnum.Received);
            var order2 = await _orderRepository.CountAsync(e => e.OrderStatus == DeliveryStatusEnum.Finish);
            return order1 + order2;
        }

        public async Task<int> GetOrderCancel()
        {
            var orderCancel = await _orderRepository.CountAsync(e => e.OrderStatus == DeliveryStatusEnum.Canceled);
            return orderCancel;
        }

        //public async Task<ExpenseReponse> GetExpenseDate(DateTime from, DateTime to)
        //{
        //    var result = await _stockReceiptRepository.GetAsync(e => e.EntryDate >= from && e.EntryDate <= to);

        //    var totalExpense = result.Sum(e => e.Total);

        //    return new ExpenseReponse
        //    {
        //        ExpenseList = result,
        //        Total = totalExpense,
        //    };

        //}
    }
}
