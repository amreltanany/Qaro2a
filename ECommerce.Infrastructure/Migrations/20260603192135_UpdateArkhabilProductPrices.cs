using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateArkhabilProductPrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE [Products]
                SET [Price] = 11.00
                WHERE [Name] = N'ارخبيل الملوك'
                  AND [Author] = N'قروءه';

                UPDATE [Products]
                SET [Price] = 130.00
                WHERE [Name] = N'ارخبيل الملوك'
                  AND [Author] = N'ممدوح عماد';

                UPDATE [Products]
                SET [Price] = 130.00
                WHERE [Name] = N'ارخبيل الملوك'
                  AND [Price] = 120.00;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE [Products]
                SET [Price] = 11.00
                WHERE [Name] = N'ارخبيل الملوك'
                  AND [Author] = N'قروءه';

                UPDATE [Products]
                SET [Price] = 120.00
                WHERE [Name] = N'ارخبيل الملوك'
                  AND [Author] = N'ممدوح عماد';
                """);
        }
    }
}
