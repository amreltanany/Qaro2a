using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GuestCheckout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Two older migrations (AddCartItemTable, AllowGuestOrderUserIdNullable) have no Designer file, so EF never
            // runs them on a fresh database. Make sure their end result exists here, without breaking databases that
            // already have it.
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[CartItem]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CartItem] (
        [Id] int NOT NULL IDENTITY(1,1),
        [UserId] nvarchar(450) NOT NULL,
        [ProductId] int NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [Quantity] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CartItem] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CartItem_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id])
    );
    CREATE INDEX [IX_CartItem_ProductId] ON [dbo].[CartItem] ([ProductId]);
    CREATE INDEX [IX_CartItem_UserId] ON [dbo].[CartItem] ([UserId]);
END");

            // A cart row now belongs to a user id OR a guest key, so it is no longer a foreign key to users.
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CartItem_AspNetUsers_UserId')
    ALTER TABLE [dbo].[CartItem] DROP CONSTRAINT [FK_CartItem_AspNetUsers_UserId];");

            // Guest orders have no user.
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "Orders");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItem_AspNetUsers_UserId",
                table: "CartItem",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
