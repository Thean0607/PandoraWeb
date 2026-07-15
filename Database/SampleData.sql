    USE [PandoraDB];
    GO

    -- Xóa dữ liệu cũ nếu có (cẩn thận khi chạy lệnh này trên môi trường thật)
    -- DELETE FROM ProductVariants;
    -- DELETE FROM Products;
    -- DELETE FROM Categories;
    -- DBCC CHECKIDENT ('Products', RESEED, 0);
    -- DBCC CHECKIDENT ('Categories', RESEED, 0);

    -- Insert Danh mục (Categories)
    INSERT INTO [Categories] ([CategoryName]) VALUES
    (N'Nhẫn'),
    (N'Dây Chuyền'),
    (N'Hoa Tai'),
    (N'Lắc Tay');
    GO

    -- Declare Category IDs
    DECLARE @NhanId INT = (SELECT CategoryId FROM Categories WHERE CategoryName = N'Nhẫn');
    DECLARE @DayChuyenId INT = (SELECT CategoryId FROM Categories WHERE CategoryName = N'Dây Chuyền');
    DECLARE @HoaTaiId INT = (SELECT CategoryId FROM Categories WHERE CategoryName = N'Hoa Tai');
    DECLARE @LacTayId INT = (SELECT CategoryId FROM Categories WHERE CategoryName = N'Lắc Tay');

    -- Insert 10 Sản phẩm (Products)
    INSERT INTO [Products] ([ProductName], [Description], [CategoryId], [CollectionId], [BasePrice], [ImageUrl], [Status], [CreatedAt], [UpdatedAt])
    VALUES
    -- 1. Nhẫn
    (N'Nhẫn Vương Miện Vàng Hồng', N'Nhẫn vương miện mạ vàng hồng 14k với đính đá lấp lánh.', @NhanId, NULL, 2500000.00, 'https://images.unsplash.com/photo-1605100804763-247f67b6348e?q=80&w=600&auto=format&fit=crop', N'Active', GETDATE(), GETDATE()),
    (N'Nhẫn Bạc Ý Cổ Điển', N'Nhẫn bạc Ý 925 kiểu dáng cổ điển, thanh lịch và tối giản.', @NhanId, NULL, 1200000.00, 'https://images.unsplash.com/photo-1599643478514-4a4e02632616?q=80&w=600&auto=format&fit=crop', N'Active', GETDATE(), GETDATE()),
    (N'Nhẫn Đính Đá Trái Tim', N'Nhẫn đính đá kim cương nhân tạo hình trái tim rực rỡ.', @NhanId, NULL, 3100000.00, 'https://images.unsplash.com/photo-1535632066927-ab7c9ab60908?q=80&w=600&auto=format&fit=crop', N'Active', GETDATE(), GETDATE()),

    -- 2. Dây Chuyền
    (N'Dây Chuyền Bạc Mặt Trăng', N'Dây chuyền bạc 925 mặt trăng khuyết và đính đá sáng.', @DayChuyenId, NULL, 2100000.00, 'https://images.unsplash.com/photo-1535632066927-ab7c9ab60908?q=80&w=600&auto=format&fit=crop', N'Active', GETDATE(), GETDATE()),
    (N'Dây Chuyền Ngọc Trai Sang Trọng', N'Dây chuyền vàng đính ngọc trai nước ngọt nuôi cấy.', @DayChuyenId, NULL, 4500000.00, 'https://images.unsplash.com/photo-1599643478514-4a4e02632616?q=80&w=600&auto=format&fit=crop', N'Active', GETDATE(), GETDATE()),

    -- 3. Hoa Tai
    (N'Hoa Tai Nụ Đá Saphire', N'Hoa tai dạng nụ đính đá Saphire xanh lam lấp lánh.', @HoaTaiId, NULL, 1850000.00, 'https://images.unsplash.com/photo-1535632787350-4e68ef0ac584?q=80&w=600&auto=format&fit=crop', N'Active', GETDATE(), GETDATE()),
    (N'Hoa Tai Dáng Dài Khuyên Tròn', N'Hoa tai bạc thiết kế khuyên tròn lồng vào nhau.', @HoaTaiId, NULL, 1500000.00, 'https://images.unsplash.com/photo-1605100804763-247f67b6348e?q=80&w=600&auto=format&fit=crop', N'Active', GETDATE(), GETDATE()),
    (N'Hoa Tai Vàng Hồng Trái Tim', N'Thiết kế trái tim cách điệu ngọt ngào mạ vàng hồng 14k.', @HoaTaiId, NULL, 2750000.00, 'https://images.unsplash.com/photo-1535632787350-4e68ef0ac584?q=80&w=600&auto=format&fit=crop', N'Active', GETDATE(), GETDATE()),

    -- 4. Lắc Tay
    (N'Lắc Tay Vòng Charm Bạc', N'Lắc tay chuỗi charm bằng bạc 925 đặc trưng của hãng.', @LacTayId, NULL, 1950000.00, 'https://images.unsplash.com/photo-1605100804763-247f67b6348e?q=80&w=600&auto=format&fit=crop', N'Active', GETDATE(), GETDATE()),
    (N'Lắc Tay Kim Loại Trơn Khắc Tên', N'Lắc tay kim loại kiểu trơn, có thể khắc tên theo yêu cầu.', @LacTayId, NULL, 2200000.00, 'https://images.unsplash.com/photo-1599643478514-4a4e02632616?q=80&w=600&auto=format&fit=crop', N'Active', GETDATE(), GETDATE());
    GO

    PRINT 'Inserted 10 sample products successfully!';
