using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategories : Migration
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        }
    }
}
