using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedArkhabilElMoloukProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @CategoryId INT;

                SELECT @CategoryId = [Id]
                FROM [Categories]
                WHERE [Name] = N'فانتازيا';

                IF @CategoryId IS NULL
                BEGIN
                    INSERT INTO [Categories] ([Name], [CreatedAt])
                    VALUES (N'فانتازيا', GETUTCDATE());

                    SET @CategoryId = CAST(SCOPE_IDENTITY() AS INT);
                END

                IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Name] = N'ارخبيل الملوك')
                BEGIN
                    INSERT INTO [Products] (
                        [Name],
                        [Price],
                        [Stock],
                        [Description],
                        [CategoryId],
                        [ImageUrl],
                        [PublishDate],
                        [Author],
                        [TopRated],
                        [CreatedAt])
                    VALUES (
                        N'ارخبيل الملوك',
                        11.00,
                        1,
                        N'ارخبيل الملوك',
                        @CategoryId,
                        N'/images/products/arkhabil-el-molouk.jpeg',
                        '2026-06-03',
                        N'قروءه',
                        0,
                        GETUTCDATE());
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [Products]
                WHERE [Name] = N'ارخبيل الملوك';
                """);
        }
    }
}
