using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderDeliveryFee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryFee",
                table: "Orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 29, 15, 7, 29, 216, DateTimeKind.Utc).AddTicks(884));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 29, 15, 7, 29, 216, DateTimeKind.Utc).AddTicks(936));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryFee",
                table: "Orders");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 1, 13, 21, 41, 982, DateTimeKind.Utc).AddTicks(2903));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 1, 13, 21, 41, 982, DateTimeKind.Utc).AddTicks(3001));
        }
    }
}
