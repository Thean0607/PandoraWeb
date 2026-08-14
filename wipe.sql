USE PandoraDB;

-- Delete dependent tables first to avoid FK constraints
DELETE FROM [Wishlists];
DELETE FROM [OrderItems];
DELETE FROM [Orders];
DELETE FROM [CartItems];
DELETE FROM [Carts];
DELETE FROM [Reviews];
DELETE FROM [Addresses];

-- Delete main tables
DELETE FROM [Customers];
DELETE FROM [Employees];

-- Ensure Admin role exists
IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE RoleName = 'Admin')
BEGIN
    INSERT INTO [Roles] (RoleName) VALUES ('Admin');
END

DECLARE @AdminRoleId INT = (SELECT TOP 1 RoleId FROM [Roles] WHERE RoleName = 'Admin');

-- SHA256 of "123456" is "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92"
INSERT INTO [Employees] (FullName, Email, PasswordHash, RoleId, Status)
VALUES (N'Quản Trị Viên', 'admin@pandora.vn', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', @AdminRoleId, 'active');

PRINT 'Data wiped and Admin account created successfully.';
