using System;
using Microsoft.EntityFrameworkCore;
using DummyProject.Models;
using DummyProject.Models.JunctionModels;
using DummyProject.Models.DomainModel;

namespace DummyProject.Data
{
    public class SqlDbContext : DbContext
    {
        public SqlDbContext(DbContextOptions<SqlDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartProduct> CartProducts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Order -> Buyer relationship
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Buyer)
                .WithMany(b => b.Orders)
                .HasForeignKey(o => o.BuyerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Product -> Seller relationship
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Seller)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Address -> Buyer relationship
            modelBuilder.Entity<Address>()
                .HasOne(a => a.Buyer)
                .WithMany(b => b.Addresses)
                .HasForeignKey(a => a.BuyerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Cart -> Buyer relationship
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Buyer)
                .WithOne(b => b.Cart)
                .HasForeignKey<Cart>(c => c.BuyerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure many-to-many relationship for CartProduct
            modelBuilder.Entity<CartProduct>()
                .HasKey(cp => cp.CartProductId);

            modelBuilder.Entity<CartProduct>()
                .HasOne(cp => cp.Cart)
                .WithMany(c => c.CartProducts)
                .HasForeignKey(cp => cp.CartId);

            modelBuilder.Entity<CartProduct>()
                .HasOne(cp => cp.Product)
                .WithMany(p => p.Carts)
                .HasForeignKey(cp => cp.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // Enable cascading delete for Product -> CartProduct

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Buyer)  // Ensure Buyer relationship exists
                .WithMany(b => b.Orders)
                .HasForeignKey(o => o.BuyerId)
                .OnDelete(DeleteBehavior.Cascade);
            
        }
        
    }
}
