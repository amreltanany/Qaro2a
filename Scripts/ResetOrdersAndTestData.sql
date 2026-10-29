-- Wipes orders, carts, wishlists, publish submissions, contact messages and test accounts, and restarts
-- the Order / OrderItem ids from 1. Keeps real users, categories and products.
-- Run on a database where you are sure nothing in it is real customer data you still need.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DELETE FROM [OrderItem];
DELETE FROM [Orders];
DELETE FROM [CartItem];
DELETE FROM [WishlistItem];
DELETE FROM [Publish];
DELETE FROM [Contact];

-- test accounts created by automated checks (example.test addresses)
DELETE FROM [AspNetUsers] WHERE [Email] LIKE N'%@example.test';

DBCC CHECKIDENT ('[OrderItem]', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[Orders]', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[Publish]', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[Contact]', RESEED, 0) WITH NO_INFOMSGS;

-- every book back to its seeded stock
UPDATE [Products] SET [Stock] = 10;

COMMIT;
SELECT 'Orders' AS [table], COUNT(*) AS [rows] FROM [Orders]
UNION ALL SELECT 'OrderItem', COUNT(*) FROM [OrderItem]
UNION ALL SELECT 'Users', COUNT(*) FROM [AspNetUsers]
UNION ALL SELECT 'Products', COUNT(*) FROM [Products]
UNION ALL SELECT 'Categories', COUNT(*) FROM [Categories];
