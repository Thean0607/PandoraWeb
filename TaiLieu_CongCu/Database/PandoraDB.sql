-- Script tạo bảng cơ sở dữ liệu PandoraWeb
-- Hỗ trợ đầy đủ tính năng: Sản phẩm, Biến thể (Size/Chất liệu), Giỏ hàng, Đơn hàng, Khuyến mãi, Khách hàng...

IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'PandoraDB')
BEGIN
    CREATE DATABASE [PandoraDB];
END
GO

USE [PandoraDB];
GO

CREATE TABLE [Roles] (
    [RoleId] INT IDENTITY(1,1) PRIMARY KEY,
    [RoleName] NVARCHAR(100) NOT NULL,
    [Permissions] NVARCHAR(MAX) NULL
);

CREATE TABLE [Employees] (
    [EmployeeId] INT IDENTITY(1,1) PRIMARY KEY,
    [FullName] NVARCHAR(150) NOT NULL,
    [Email] NVARCHAR(150) NOT NULL,
    [PasswordHash] NVARCHAR(255) NOT NULL,
    [RoleId] INT NOT NULL,
    [Status] NVARCHAR(20) NULL,
    CONSTRAINT [FK_Employees_Roles] FOREIGN KEY ([RoleId]) REFERENCES [Roles]([RoleId])
);

CREATE TABLE [Customers] (
    [CustomerId] INT IDENTITY(1,1) PRIMARY KEY,
    [FullName] NVARCHAR(150) NOT NULL,
    [Email] NVARCHAR(150) NOT NULL,
    [PhoneNumber] NVARCHAR(20) NULL,
    [PasswordHash] NVARCHAR(255) NULL,
    [Gender] NVARCHAR(10) NULL,
    [DateOfBirth] DATETIME NULL,
    [Status] NVARCHAR(20) NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE [Addresses] (
    [AddressId] INT IDENTITY(1,1) PRIMARY KEY,
    [CustomerId] INT NOT NULL,
    [ReceiverName] NVARCHAR(150) NOT NULL,
    [PhoneNumber] NVARCHAR(20) NOT NULL,
    [StreetAddress] NVARCHAR(255) NOT NULL,
    [City] NVARCHAR(100) NOT NULL,
    [District] NVARCHAR(100) NOT NULL,
    [Ward] NVARCHAR(100) NOT NULL,
    [IsDefault] BIT NOT NULL DEFAULT 0,
    CONSTRAINT [FK_Addresses_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId])
);

CREATE TABLE [Categories] (
    [CategoryId] INT IDENTITY(1,1) PRIMARY KEY,
    [CategoryName] NVARCHAR(100) NOT NULL
);

CREATE TABLE [Collections] (
    [CollectionId] INT IDENTITY(1,1) PRIMARY KEY,
    [CollectionName] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [ImageUrl] NVARCHAR(500) NULL
);

CREATE TABLE [Products] (
    [ProductId] INT IDENTITY(1,1) PRIMARY KEY,
    [ProductName] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [CategoryId] INT NOT NULL,
    [CollectionId] INT NULL,
    [BasePrice] DECIMAL(18,2) NOT NULL,
    [ImageUrl] NVARCHAR(500) NULL,
    [Status] NVARCHAR(20) NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_Products_Categories] FOREIGN KEY ([CategoryId]) REFERENCES [Categories]([CategoryId]),
    CONSTRAINT [FK_Products_Collections] FOREIGN KEY ([CollectionId]) REFERENCES [Collections]([CollectionId])
);

CREATE TABLE [Materials] (
    [MaterialId] INT IDENTITY(1,1) PRIMARY KEY,
    [MaterialName] NVARCHAR(100) NOT NULL
);

CREATE TABLE [Sizes] (
    [SizeId] INT IDENTITY(1,1) PRIMARY KEY,
    [SizeValue] NVARCHAR(50) NOT NULL
);

CREATE TABLE [ProductVariants] (
    [VariantId] INT IDENTITY(1,1) PRIMARY KEY,
    [ProductId] INT NOT NULL,
    [SizeId] INT NULL,
    [MaterialId] INT NULL,
    [SKU] NVARCHAR(100) NULL,
    [Stock] INT NOT NULL DEFAULT 0,
    [PriceAdjustment] DECIMAL(18,2) NOT NULL DEFAULT 0,
    CONSTRAINT [FK_ProductVariants_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products]([ProductId]),
    CONSTRAINT [FK_ProductVariants_Sizes] FOREIGN KEY ([SizeId]) REFERENCES [Sizes]([SizeId]),
    CONSTRAINT [FK_ProductVariants_Materials] FOREIGN KEY ([MaterialId]) REFERENCES [Materials]([MaterialId])
);

CREATE TABLE [ProductImages] (
    [ImageId] INT IDENTITY(1,1) PRIMARY KEY,
    [ProductId] INT NOT NULL,
    [ImageUrl] NVARCHAR(500) NOT NULL,
    [IsPrimary] BIT NOT NULL DEFAULT 0,
    [DisplayOrder] INT NOT NULL DEFAULT 0,
    CONSTRAINT [FK_ProductImages_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products]([ProductId])
);

CREATE TABLE [Reviews] (
    [ReviewId] INT IDENTITY(1,1) PRIMARY KEY,
    [ProductId] INT NOT NULL,
    [CustomerId] INT NOT NULL,
    [Rating] INT NOT NULL,
    [Comment] NVARCHAR(1000) NULL,
    [ReviewDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [Status] NVARCHAR(20) NULL,
    CONSTRAINT [FK_Reviews_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products]([ProductId]),
    CONSTRAINT [FK_Reviews_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId])
);

CREATE TABLE [Wishlists] (
    [WishlistId] INT IDENTITY(1,1) PRIMARY KEY,
    [CustomerId] INT NOT NULL,
    [ProductId] INT NOT NULL,
    [AddedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_Wishlists_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]),
    CONSTRAINT [FK_Wishlists_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products]([ProductId])
);

CREATE TABLE [Carts] (
    [CartId] INT IDENTITY(1,1) PRIMARY KEY,
    [CustomerId] INT NOT NULL,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_Carts_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId])
);

CREATE TABLE [CartItems] (
    [CartItemId] INT IDENTITY(1,1) PRIMARY KEY,
    [CartId] INT NOT NULL,
    [VariantId] INT NOT NULL,
    [Quantity] INT NOT NULL,
    CONSTRAINT [FK_CartItems_Carts] FOREIGN KEY ([CartId]) REFERENCES [Carts]([CartId]),
    CONSTRAINT [FK_CartItems_ProductVariants] FOREIGN KEY ([VariantId]) REFERENCES [ProductVariants]([VariantId])
);

CREATE TABLE [Promotions] (
    [PromotionId] INT IDENTITY(1,1) PRIMARY KEY,
    [Code] NVARCHAR(50) NOT NULL,
    [DiscountPercentage] INT NULL,
    [DiscountAmount] DECIMAL(18,2) NULL,
    [StartDate] DATETIME NOT NULL,
    [EndDate] DATETIME NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1
);

CREATE TABLE [Orders] (
    [OrderId] INT IDENTITY(1,1) PRIMARY KEY,
    [CustomerId] INT NOT NULL,
    [ShippingAddressId] INT NULL,
    [PromotionId] INT NULL,
    [TotalAmount] DECIMAL(18,2) NOT NULL,
    [ShippingFee] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [DiscountAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [OrderStatus] NVARCHAR(50) NULL,
    [PaymentMethod] NVARCHAR(50) NULL,
    [PaymentStatus] NVARCHAR(50) NULL,
    [Notes] NVARCHAR(500) NULL,
    [OrderDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_Orders_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([CustomerId]),
    CONSTRAINT [FK_Orders_Addresses] FOREIGN KEY ([ShippingAddressId]) REFERENCES [Addresses]([AddressId]),
    CONSTRAINT [FK_Orders_Promotions] FOREIGN KEY ([PromotionId]) REFERENCES [Promotions]([PromotionId])
);

CREATE TABLE [OrderItems] (
    [OrderItemId] INT IDENTITY(1,1) PRIMARY KEY,
    [OrderId] INT NOT NULL,
    [VariantId] INT NOT NULL,
    [Quantity] INT NOT NULL,
    [UnitPrice] DECIMAL(18,2) NOT NULL,
    CONSTRAINT [FK_OrderItems_Orders] FOREIGN KEY ([OrderId]) REFERENCES [Orders]([OrderId]),
    CONSTRAINT [FK_OrderItems_ProductVariants] FOREIGN KEY ([VariantId]) REFERENCES [ProductVariants]([VariantId])
);

-- =================================================================
-- Script Cấp 3 Tài Khoản (Admin, Manager, Customer) cho PandoraWeb
-- =================================================================

-- 1. Xoá dữ liệu cũ (nếu muốn reset, bỏ comment)
-- DELETE FROM Employees;
-- DELETE FROM Roles;
-- DELETE FROM Customers;

-- 2. Tạo 2 quyền (Role) cho Nhân viên/Admin
SET IDENTITY_INSERT Roles ON;
INSERT INTO Roles (RoleId, RoleName, Permissions) VALUES 
(1, N'Admin', '["All"]'),
(2, N'Manager', '["ManageProducts", "ManageOrders"]');
SET IDENTITY_INSERT Roles OFF;

-- 3. Tạo 2 tài khoản Nhân viên (Admin và Manager)
-- Lưu ý: PasswordHash tạm để dạng plain text '123456'
SET IDENTITY_INSERT Employees ON;
INSERT INTO Employees (EmployeeId, FullName, Email, PasswordHash, RoleId, Status) VALUES 
(1, N'Quản Trị Viên', 'admin@pandora.com', '123456', 1, 'active'),
(2, N'Quản Lý Cửa Hàng', 'manager@pandora.com', '123456', 2, 'active');
SET IDENTITY_INSERT Employees OFF;

-- 4. Tạo 1 tài khoản Khách hàng (Customer)
SET IDENTITY_INSERT Customers ON;
INSERT INTO Customers (CustomerId, FullName, Email, PhoneNumber, PasswordHash, Gender, Status, CreatedAt) VALUES 
(1, N'Khách Hàng Vip', 'guest@gmail.com', '0901234567', '123456', 'Other', 'active', GETDATE());
SET IDENTITY_INSERT Customers OFF;

PRINT N'Đã khởi tạo thành công 3 tài khoản!';
