using Microsoft.EntityFrameworkCore;
using MyStore.Data;
using MyStore.Models;
using MyStore.Repository.CommonRepository;
using MyStore.Services;

namespace MyStore.Repository.Users
{
    public class UserRepository : CommonRepository<User> , IUserRepository
    {
        private readonly CompanyDBContext _context;

        public UserRepository(CompanyDBContext context) : base(context) {
            _context = context; 
        }

        //public async Task<int> CountAsync()
        //{
        //    return await _context.Users.CountAsync();
        //}

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

        public async Task<IEnumerable<User>> GetAllUserAsync(int page, int pageSize, string? search)
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
