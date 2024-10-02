using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyStore.Models;

namespace MyStore.Data
{
    public class CompanyDBContext : IdentityDbContext<User>
    {
        public CompanyDBContext(DbContextOptions<CompanyDBContext> options) : base(options) { }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Caterories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<DeliveryStatus> DeliveryStatuses { get; set; }
        public DbSet<DeliveryAddress> DeliveryAddress { get; set; }


        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IBaseEntity 
                && (e.State == EntityState.Added || e.State == EntityState.Modified));
            foreach (var entry in entries)
            {
                if(entry.State == EntityState.Added)
                {
                    ((IBaseEntity)entry.Entity).CreatedAt = DateTime.UtcNow;
                }
                if(entry.State == EntityState.Modified)
                {
                    ((IBaseEntity)entry.Entity).UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            UpdateTimestamps();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

    }
}

