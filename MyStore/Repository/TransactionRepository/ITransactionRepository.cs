using Microsoft.EntityFrameworkCore.Storage;

namespace MyStore.Repository.TransactionRepository
{
    public interface ITransactionRepository
    {
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
