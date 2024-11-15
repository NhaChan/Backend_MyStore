using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using MyStore.Data;

namespace MyStore.Repository.TransactionRepository
{
    public class TransactionRepository(CompanyDBContext dbContext) : ITransactionRepository
    {
        private readonly CompanyDBContext _dbContext = dbContext;

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _dbContext.Database.BeginTransactionAsync();
        }

        public Task CommitTransactionAsync()
        {
            return _dbContext.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _dbContext.Database.RollbackTransactionAsync();
        }
    }
}
