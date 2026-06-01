-- Run once on Azure SQL if My Account fails with invalid column DeliveryFee.
-- Or redeploy: the app applies EF migrations on startup.

IF COL_LENGTH('Orders', 'DeliveryFee') IS NULL
BEGIN
    ALTER TABLE [Orders] ADD [DeliveryFee] decimal(18,2) NOT NULL CONSTRAINT [DF_Orders_DeliveryFee] DEFAULT (0);
END

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260529150732_AddOrderDeliveryFee')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260529150732_AddOrderDeliveryFee', N'8.0.22');
