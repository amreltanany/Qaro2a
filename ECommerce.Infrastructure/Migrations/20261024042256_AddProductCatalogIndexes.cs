using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCatalogIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_CreatedAt",
                table: "Products",
                columns: new[] { "CategoryId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CreatedAt",
                table: "Products",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Price",
                table: "Products",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_Products_PublishDate",
                table: "Products",
                column: "PublishDate");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TopRated_CreatedAt",
                table: "Products",
                columns: new[] { "TopRated", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId_CreatedAt",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CreatedAt",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Price",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_PublishDate",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_TopRated_CreatedAt",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");
        }
    }
}
