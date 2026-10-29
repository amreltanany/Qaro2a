using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Start the catalogue from id 1. Order history is protected: if any order still points to a
            // product, the DELETE below fails and the migration rolls back instead of losing orders.
            migrationBuilder.Sql("DELETE FROM [CartItem]");
            migrationBuilder.Sql("DELETE FROM [WishlistItem]");
            migrationBuilder.Sql("DELETE FROM [Products]");
            migrationBuilder.Sql("DELETE FROM [Categories]");
            migrationBuilder.Sql("DBCC CHECKIDENT ('[Products]', RESEED, 0)");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "اجتماعي ودراما" },
                    { 2, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "فانتازيا وخيال علمي" },
                    { 3, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "جريمة وغموض وإثارة" },
                    { 4, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "رومانسي" },
                    { 5, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "تدريب وهوايات" },
                    { 6, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "نفسي" },
                    { 7, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "تاريخي وسير" },
                    { 8, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "فكر وثقافة" },
                    { 9, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "رعب" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Author", "CategoryId", "CreatedAt", "Description", "ImageUrl", "Name", "Price", "PublishDate", "Stock", "TopRated" },
                values: new object[,]
                {
                    { 1, "نوران بسيوني", 1, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "بعد فارق — نوران بسيوني. من إصدارات قروءه.", "/images/products/baad-fariq.webp", "بعد فارق", 130m, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), 10, true },
                    { 2, "ممدوح عماد", 2, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "أرخبيل الملوك — ممدوح عماد. من إصدارات قروءه.", "/images/products/arkhabil-el-molouk.webp", "أرخبيل الملوك", 130m, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), 10, true },
                    { 3, "آية المنشاوي", 1, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "طيف نجاة — آية المنشاوي. من إصدارات قروءه.", "/images/products/tayf-najah.webp", "طيف نجاة", 130m, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), 10, true },
                    { 4, "آية المنشاوي", 1, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "لوحة أخيرة — آية المنشاوي. من إصدارات قروءه.", "/images/products/lawha-akhira.webp", "لوحة أخيرة", 130m, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), 10, true },
                    { 5, "ممدوح عماد، نوران بسيوني، رنا رؤوف، آية المنشاوي", 1, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "حلقة الذنب — ممدوح عماد، نوران بسيوني، رنا رؤوف، آية المنشاوي. من إصدارات قروءه.", "/images/products/halaqat-al-dhanb.webp", "حلقة الذنب", 130m, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), 10, true },
                    { 6, "رنا رؤوف", 4, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), "طلبك مرفوض — رنا رؤوف. من إصدارات قروءه.", "/images/products/talabak-marfoud.webp", "طلبك مرفوض", 130m, new DateTime(2026, 10, 29, 0, 0, 0, 0, DateTimeKind.Utc), 10, true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "Products", keyColumn: "Id", keyValues: new object[] { 1, 2, 3, 4, 5, 6 });
            migrationBuilder.DeleteData(table: "Categories", keyColumn: "Id", keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        }
    }
}
