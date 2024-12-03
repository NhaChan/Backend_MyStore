using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using MyStore.Repository.CommonRepository;
using MyStore.Services;
using System.Linq.Expressions;

namespace MyStore.Repository.Users
{
    public class UserRepository(CompanyDBContext context) : CommonRepository<User>(context) , IUserRepository
    {
        private readonly CompanyDBContext _context = context;

        public override async Task<IEnumerable<User>> GetPageOrderByDescendingAsync<TKey>(int page, int pageSize, Expression<Func<User, bool>>? expression, Expression<Func<User, TKey>> orderByDesc)
        => expression == null
            ? await _context.Users
                .Paginate(page, pageSize)
                .OrderByDescending(orderByDesc)
                .Include(x => x.UserRoles)
                    .ThenInclude(e => e.Role)
                .ToArrayAsync()
            : await _context.Users
                .Where(expression)
                .Paginate(page, pageSize)
                .OrderByDescending(orderByDesc)
                .Include(x => x.UserRoles)
                    .ThenInclude(e => e.Role)
                .ToArrayAsync();

        public async Task<int> CountAsync(string search)
        {
            return await _context.Users
            .Where(e => e.Id.ToString().Contains(search)
                    || (e.FullName != null && e.FullName.Contains(search))
                    || (e.Email != null && e.Email.Contains(search))
                    || (e.PhoneNumber != null && e.PhoneNumber.Contains(search)))
                .CountAsync();
        }

        public async Task<IEnumerable<User>> GetAllUserAsync(int page, int pageSize)
        {
            return await _context.Users
                .Paginate(page, pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAllUserAsync(int page, int pageSize, string search)
        {
            return await _context.Users
                .Where(e => e.Id.ToString().Contains(search)
                || (e.FullName != null && e.FullName.Contains(search))
                || (e.Email != null && e.Email.Contains(search)) 
                || (e.PhoneNumber != null && e.PhoneNumber.Contains(search)))
                .Paginate(page, pageSize)
                .ToListAsync();
        }
    }
}
