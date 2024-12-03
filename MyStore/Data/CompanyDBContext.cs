using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyStore.Models;
using System.Data;

namespace MyStore.Data
{
    public class CompanyDBContext : IdentityDbContext<User, Role, string, IdentityUserClaim<string>, UserRole,
        IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public CompanyDBContext(DbContextOptions<CompanyDBContext> options) : base(options) { }
        public virtual DbSet<Brand> Brands { get; set; }
        public virtual DbSet<Category> Caterories { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        public virtual DbSet<CartItem> CartItems { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }
        public virtual DbSet<DeliveryStatus> DeliveryStatuses { get; set; }
        public virtual DbSet<DeliveryAddress> DeliveryAddress { get; set; }
        public virtual DbSet<ProductFavorite> ProductFavorites { get; set; }
        public virtual DbSet<ProductReview> ProductReviews { get; set; }
        public virtual DbSet<StockReceipt> StockReceipts { get; set; }
        public virtual DbSet<StockReceiptDetail> StockReceiptDetails { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<LogDetail> LogDetails { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>(e =>
            {
                e.HasMany(x => x.UserRoles)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .IsRequired();
            });
            builder.Entity<Role>(e =>
            {
                e.HasMany(x => x.UserRoles)
                .WithOne(x => x.Role)
                .HasForeignKey(x => x.RoleId)
                .IsRequired();
            });
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IBaseEntity
                && (e.State == EntityState.Added || e.State == EntityState.Modified));
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    ((IBaseEntity)entry.Entity).CreatedAt = DateTime.Now;
                }
                if (entry.State == EntityState.Modified)
                {
                    ((IBaseEntity)entry.Entity).UpdatedAt = DateTime.Now;
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

