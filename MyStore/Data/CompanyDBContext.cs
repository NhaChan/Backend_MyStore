using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyStore.Models;

namespace MyStore.Data
{
    public class CompanyDBContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public CompanyDBContext(DbContextOptions<CompanyDBContext> options) : base(options) { }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Caterory> Caterories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }

    }
}
