USE PandoraDB;

-- Delete dependent tables first
DELETE FROM [Wishlists];
DELETE FROM [OrderItems];
DELETE FROM [CartItems];
DELETE FROM [Reviews];

DELETE FROM [Orders];
DELETE FROM [Carts];

DELETE FROM [Addresses];

DELETE FROM [ProductImages];
DELETE FROM [ProductVariants];

-- Delete main tables
DELETE FROM [Products];
DELETE FROM [Categories];
DELETE FROM [Collections];
DELETE FROM [Materials];
DELETE FROM [Sizes];

DELETE FROM [Promotions];
DELETE FROM [Banners];
DELETE FROM [Pages];
DELETE FROM [BlogPosts];
DELETE FROM [Faqs];

DELETE FROM [Customers];
DELETE FROM [Employees];

-- Ensure Admin role exists
IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE RoleName = 'Admin')
BEGIN
    INSERT INTO [Roles] (RoleName) VALUES ('Admin');
END

DECLARE @AdminRoleId INT = (SELECT TOP 1 RoleId FROM [Roles] WHERE RoleName = 'Admin');

INSERT INTO [Employees] (FullName, Email, PasswordHash, RoleId, Status)
VALUES (N'Quản Trị Viên', 'admin@pandora.vn', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', @AdminRoleId, 'active');

PRINT 'All data wiped and Admin account recreated.';
