using MyStore.Enumerations;
using MyStore.Response;

namespace MyStore.Services.Expenses
{
    public interface IExpenseService
    {
        Task<int> GetCountUser();
        Task<int> GetCountProduct();
        Task<int> GetCountOrder();
        Task<int> GetOrderCancel();
        //Task<ExpenseReponse> GetExpenseDate(DateTime from, DateTime to);

    }
}
