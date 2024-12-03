using System.Linq.Expressions;

namespace MyStore.Repository.CommonRepository
{
    public interface ICommonRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> FindAsync(params object?[]? keyValues);
        Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> expression);
        Task<T?> SingleAsync(Expression<Func<T, bool>> expression);
        Task AddAsync(T entity);
        Task AddAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task UpdateAsync(IEnumerable<T> entities);
        Task DeleteAsync(T entity);
        Task DeleteAsync(IEnumerable<T> entities);
        Task DeleteAsync(params object?[]? keyValues);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> expression);
        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> expression);
        Task<T?> FindAsyncCart(Expression<Func<T, bool>> expression);
        Task<IEnumerable<T>> GetPagedAsync<TKey>(int page, int pageSize, Expression<Func<T, bool>>? expression, Expression<Func<T, TKey>> orderBy);
        Task<IEnumerable<T>> GetPageOrderByDescendingAsync<TKey>(int page, int pageSize, Expression<Func<T, bool>>? expression, Expression<Func<T, TKey>> orderByDesc);
    }
}
