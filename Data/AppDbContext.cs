using Microsoft.EntityFrameworkCore;
using FoodMgmt.Models;

namespace FoodMgmt.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // decimal precision for prices
            modelBuilder.Entity<FoodItem>().Property(f => f.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<OrderDetail>().Property(od => od.UnitPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Order>().Property(o => o.GrandTotal).HasColumnType("decimal(18,2)");

            // Prevent cascade deletes to avoid accidental data loss
            modelBuilder.Entity<FoodItem>()
                .HasOne(f => f.Category)
                .WithMany(c => c.FoodItems)
                .HasForeignKey(f => f.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FoodItem>()
                .HasOne(f => f.Supplier)
                .WithMany(s => s.FoodItems)
                .HasForeignKey(f => f.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.FoodItem)
                .WithMany(fi => fi.OrderDetails)
                .HasForeignKey(od => od.FoodItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed minimal data (adjust as needed)
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Beverages", Description = "Drinks" },
                new Category { Id = 2, Name = "Snacks", Description = "Quick bites" }
            );

            modelBuilder.Entity<Supplier>().HasData(
                new Supplier { Id = 1, Name = "Local Supplier", Phone = "9800000000", Email = "local@supplier.com" }
            );

            modelBuilder.Entity<FoodItem>().HasData(
                new FoodItem { Id = 1, Name = "Tea", Description = "Hot beverage", Price = 1.5m, Stock = 100, CategoryId = 1, SupplierId = 1 },
                new FoodItem { Id = 2, Name = "Samosa", Description = "Fried snack", Price = 0.8m, Stock = 50, CategoryId = 2, SupplierId = 1 }
            );

            modelBuilder.Entity<Customer>().HasData(
                new Customer { Id = 1, FullName = "Sample Customer", Email = "cust@example.com", Phone = "9801111111" }
            );
        }
    }
}
