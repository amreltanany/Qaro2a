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

    private static readonly DateTime SeedDate = new DateTime(2026, 10, 29, 0, 0, 0, DateTimeKind.Utc);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); 

        modelBuilder.Entity<Product>()
         .Property(p => p.Price)
         .HasPrecision(18, 2);

        // Catalogue indexes: default sort, category and top-rated listings, price and date filters.
        modelBuilder.Entity<Product>().HasIndex(p => p.CreatedAt).IsDescending();
        modelBuilder.Entity<Product>().HasIndex(p => new { p.CategoryId, p.CreatedAt });
        modelBuilder.Entity<Product>().HasIndex(p => new { p.TopRated, p.CreatedAt });
        modelBuilder.Entity<Product>().HasIndex(p => p.Price);
        modelBuilder.Entity<Product>().HasIndex(p => p.PublishDate);

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CartItem>()
            .ToTable("CartItem");

        modelBuilder.Entity<CartItem>()
            .Property(c => c.Price)
            .HasPrecision(18, 2);

        // CartItem.UserId holds either a user id or a guest key ("guest:<guid>"), so it is not a foreign key.
        modelBuilder.Entity<CartItem>().Ignore(c => c.User);
        modelBuilder.Entity<CartItem>().HasIndex(c => c.UserId);

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

        // Catalogue seed. Ids start at 1; dates are fixed so migrations stay stable.
        // Top-rated books (ids from 1). Prices other than أرخبيل الملوك are provisional.
        modelBuilder.Entity<Category>().HasData(
            new { Id = 1, Name = "اجتماعي ودراما", CreatedAt = SeedDate },
            new { Id = 2, Name = "فانتازيا وخيال علمي", CreatedAt = SeedDate },
            new { Id = 3, Name = "جريمة وغموض وإثارة", CreatedAt = SeedDate },
            new { Id = 4, Name = "رومانسي", CreatedAt = SeedDate },
            new { Id = 5, Name = "تدريب وهوايات", CreatedAt = SeedDate },
            new { Id = 6, Name = "نفسي", CreatedAt = SeedDate },
            new { Id = 7, Name = "تاريخي وسير", CreatedAt = SeedDate },
            new { Id = 8, Name = "فكر وثقافة", CreatedAt = SeedDate },
            new { Id = 9, Name = "رعب", CreatedAt = SeedDate }
        );
        modelBuilder.Entity<Product>().HasData(
            new { Id = 1, Name = "بعد فارق", Price = 130m, Stock = 10, Description = "بعد فارق — نوران بسيوني. من إصدارات قروءه.", CategoryId = 1, ImageUrl = "/images/products/baad-fariq.webp", Author = "نوران بسيوني", PublishDate = SeedDate, TopRated = true, CreatedAt = SeedDate },
            new { Id = 2, Name = "أرخبيل الملوك", Price = 130m, Stock = 10, Description = "أرخبيل الملوك — ممدوح عماد. من إصدارات قروءه.", CategoryId = 2, ImageUrl = "/images/products/arkhabil-el-molouk.webp", Author = "ممدوح عماد", PublishDate = SeedDate, TopRated = true, CreatedAt = SeedDate },
            new { Id = 3, Name = "طيف نجاة", Price = 130m, Stock = 10, Description = "طيف نجاة — آية المنشاوي. من إصدارات قروءه.", CategoryId = 1, ImageUrl = "/images/products/tayf-najah.webp", Author = "آية المنشاوي", PublishDate = SeedDate, TopRated = true, CreatedAt = SeedDate },
            new { Id = 4, Name = "لوحة أخيرة", Price = 130m, Stock = 10, Description = "لوحة أخيرة — آية المنشاوي. من إصدارات قروءه.", CategoryId = 1, ImageUrl = "/images/products/lawha-akhira.webp", Author = "آية المنشاوي", PublishDate = SeedDate, TopRated = true, CreatedAt = SeedDate },
            new { Id = 5, Name = "حلقة الذنب", Price = 130m, Stock = 10, Description = "حلقة الذنب — ممدوح عماد، نوران بسيوني، رنا رؤوف، آية المنشاوي. من إصدارات قروءه.", CategoryId = 1, ImageUrl = "/images/products/halaqat-al-dhanb.webp", Author = "ممدوح عماد، نوران بسيوني، رنا رؤوف، آية المنشاوي", PublishDate = SeedDate, TopRated = true, CreatedAt = SeedDate },
            new { Id = 6, Name = "طلبك مرفوض", Price = 130m, Stock = 10, Description = "طلبك مرفوض — رنا رؤوف. من إصدارات قروءه.", CategoryId = 4, ImageUrl = "/images/products/talabak-marfoud.webp", Author = "رنا رؤوف", PublishDate = SeedDate, TopRated = true, CreatedAt = SeedDate }
        );
    }
}
