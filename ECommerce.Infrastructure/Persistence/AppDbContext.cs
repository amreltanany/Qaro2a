using ECommerce.Application.DTOs.Order;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<User>
{
    //database context that EF Core uses
   
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }


    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Publish> Publishes => Set<Publish>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<Contact> Contacts => Set<Contact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); 

        modelBuilder.Entity<Product>()
         .Property(p => p.Price)
         .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CartItem>()
            .ToTable("CartItem");

        modelBuilder.Entity<CartItem>()
            .Property(c => c.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CartItem>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);

        modelBuilder.Entity<CartItem>()
            .HasOne(c => c.Product)
            .WithMany()
            .HasForeignKey(c => c.ProductId)
            .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);

        modelBuilder.Entity<WishlistItem>()
            .ToTable("WishlistItem");

        modelBuilder.Entity<WishlistItem>()
            .HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);

        modelBuilder.Entity<WishlistItem>()
            .HasOne(w => w.Product)
            .WithMany()
            .HasForeignKey(w => w.ProductId)
            .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);

        // Map relationship explicitly (optional for guest checkout)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .IsRequired(false);

        modelBuilder.Entity<Order>()
            .Property(o => o.DeliveryFee)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Publish>()
            .ToTable("Publish");
        modelBuilder.Entity<Contact>()
            .ToTable("Contact");
        modelBuilder.Entity<Publish>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);

        modelBuilder.Entity<Category>().HasData(
             new
             {
                 Id = 1,
                 Name = "Electronics",
                 CreatedAt = DateTime.UtcNow
             }
         );
        modelBuilder.Entity<Product>().HasData(
            new
            {
                Id = 1,
                Name = "product",
                Price = 10.99m,
                Stock = 10,
                Description = "Product Description",
                CategoryId = 1,
                Author = "System",
                PublishDate = new DateTime(2026, 1, 1),
                TopRated = false,
                CreatedAt = DateTime.UtcNow // MUST add this
            }
        );
    }
}
