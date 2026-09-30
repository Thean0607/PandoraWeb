# Hướng Dẫn Cài Đặt Dự Án PandoraWeb

## 1. Yêu cầu hệ thống
- Visual Studio 2019 hoặc 2022.
- SQL Server Management Studio (SSMS) (Bản 2014 trở lên).
- .NET Framework (Khuyên dùng 4.7.2 trở lên).

## 2. Khôi phục Cơ sở dữ liệu (Database)
Các file Database (Cơ sở dữ liệu) đã được trích xuất sẵn và lưu trong thư mục **`TaiLieu_CongCu/Database`** của dự án. Bạn có thể chọn 1 trong 2 cách sau để khôi phục:

### Cách 1: Khôi phục bằng file Backup (.bak) - Khuyên dùng
1. Mở **SQL Server Management Studio (SSMS)** và kết nối vào Server, thường là: `(localdb)\MSSQLLocalDB` (hoặc `.\SQLEXPRESS`, `localhost`).
2. Chuột phải vào mục **Databases** -> Chọn **Restore Database...**
3. Ở mục **Source**, chọn **Device** -> bấm dấu `...` -> chọn **Add**.
4. Trỏ đường dẫn đến file: `TaiLieu_CongCu\Database\backup\PandoraDB_Backup.bak` trong thư mục dự án.
5. Bấm **OK** để khôi phục. Lúc này Database mang tên `PandoraDB` sẽ xuất hiện.

### Cách 2: Tạo bằng Script SQL (.sql)
1. Mở SSMS, kết nối vào Server.
2. Tạo một Database mới trắng tinh tên là `PandoraDB`.
3. Chọn File -> Open -> File... và mở file `TaiLieu_CongCu\Database\PandoraDB.sql`. Bấm **Execute** (F5) để chạy script tạo các bảng.
4. Mở tiếp file `TaiLieu_CongCu\Database\SampleData.sql` và nhấn **Execute** để nạp dữ liệu mẫu (sản phẩm, người dùng).

## 3. Cấu hình Chuỗi kết nối (Connection String)
1. Mở file `Web.config` ở ngoài cùng của dự án bằng Visual Studio.
2. Tìm đến thẻ `<connectionStrings>`.
3. Theo cấu hình mặc định, hệ thống đang kết nối đến `(localdb)\MSSQLLocalDB`:
```xml
<add name="PandoraConnectionString" connectionString="Server=(localdb)\MSSQLLocalDB;Database=PandoraDB;Trusted_Connection=True;" providerName="System.Data.SqlClient" />
```
*(Lưu ý: Nếu ở bước 2 bạn kết nối vào Server tên khác như `.\SQLEXPRESS`, bạn phải đổi chữ `(localdb)\MSSQLLocalDB` trong chuỗi trên thành `.\SQLEXPRESS` tương ứng).*

## 4. Khởi chạy dự án
1. Nháy đúp vào file `PandoraWeb.sln` để mở toàn bộ dự án bằng Visual Studio.
2. Đợi Visual Studio load xong, nhấn chuột phải vào tên Solution (ở cửa sổ Solution Explorer) -> Chọn **Restore NuGet Packages** để tải các thư viện cần thiết.
3. Nhấn **F5** (hoặc nút Play xanh lá phía trên) để chạy website.

## 5. Tài khoản dùng thử (Gợi ý)
- Tài khoản quản trị thường lưu ở bảng `Employees`. (Ví dụ: admin@pandora.com / 123456)
- Tài khoản khách mua hàng lưu ở bảng `Customers`. 
*(Bạn có thể mở database trong SSMS để xem trực tiếp danh sách tài khoản)*
