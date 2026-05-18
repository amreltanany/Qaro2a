-- CartItem migration was never applied on Azure (orphan migration file).
-- Run once, then: dotnet ef database update (will stay in sync).

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CartItem')
BEGIN
    CREATE TABLE [CartItem] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ProductId] int NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [Quantity] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CartItem] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CartItem_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CartItem_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id])
    );

    CREATE INDEX [IX_CartItem_ProductId] ON [CartItem] ([ProductId]);
    CREATE INDEX [IX_CartItem_UserId] ON [CartItem] ([UserId]);
END

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260310000000_AddCartItemTable')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260310000000_AddCartItemTable', N'8.0.22');
