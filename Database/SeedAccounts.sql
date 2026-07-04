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
