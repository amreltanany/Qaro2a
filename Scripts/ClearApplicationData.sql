-- Clears transactional data on Azure SQL / local SQL Server.
-- Keeps: AspNetUsers, Categories, Products (stock reset for seed product).
-- Run in Azure Portal: ECommerceDb -> Query editor (or sqlcmd).

DELETE FROM [OrderItem];
DELETE FROM [Orders];
DELETE FROM [CartItem];
DELETE FROM [WishlistItem];
DELETE FROM [Publish];
DELETE FROM [Contact];

-- Restore default stock for seeded product (adjust ids if needed)
UPDATE [Products] SET [Stock] = 10 WHERE [Id] = 1;
