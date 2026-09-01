$word = New-Object -ComObject Word.Application
$word.Visible = $false
$doc = $word.Documents.Add()
$selection = $word.Selection

$selection.Font.Name = "Times New Roman"
$selection.Font.Size = 14
$selection.Font.Bold = $true
$selection.TypeText("BÁO CÁO TIẾN ĐỘ TUẦN 9`n")

$selection.Font.Size = 13
$selection.Font.Bold = $false
$selection.TypeText("Họ và tên: Nguyễn Thế An`nMSSV: 2400004657`n`n")

$selection.Font.Bold = $true
$selection.TypeText("1. Tổng quan tiến độ tuần 9:`n")
$selection.Font.Bold = $false
$selection.TypeText("Tiếp nối các tiến độ trước, trong tuần 9, dự án PandoraWeb tập trung vào việc khắc phục các lỗi (bug) còn tồn đọng trong hệ thống và hoàn thiện phân hệ Quản trị (Admin) dành cho các chương trình Marketing. Trong tuần này, em đã xử lý dứt điểm các lỗi liên quan đến cấu hình hệ thống (Windows Security chặn DLL) và lỗi truyền tải dữ liệu ở trang Phân loại khách hàng. Quan trọng nhất, em đã thiết kế và lập trình lại toàn bộ luồng quy trình của hệ thống Flash Sale trực quan hơn, giúp Admin có thể trực tiếp quản lý giá và thời hạn giảm giá sốc trên từng sản phẩm.`n`n")

$selection.Font.Bold = $true
$selection.TypeText("2. Công việc đã thực hiện trong tuần 9:`n")
$selection.Font.Bold = $false
$selection.TypeText("Các đầu việc trong tuần 9 được triển khai tập trung vào việc gỡ lỗi và tối ưu hóa quy trình quản trị:`n`n")

$selection.TypeText("- Sửa lỗi Hệ thống & Cấu hình: Cấu hình lại thư mục tạm TempBuild trong web.config để vượt qua lỗi chặn DLL (Application Control - 0x800711C7). Cập nhật .csproj để nhận diện file source mới.`n")
$selection.Font.Italic = $true
$selection.Font.Color = 255 # Red
$selection.TypeText("  [Chèn ảnh: Chụp màn hình file web.config chỗ dòng thêm thuộc tính tempDirectory=`"TempBuild`"]`n`n")
$selection.Font.Italic = $false
$selection.Font.Color = 0

$selection.TypeText("- Sửa lỗi hiển thị UI Admin: Fix lỗi đường dẫn ảnh để hiển thị chính xác `"Hình Ảnh Chính`" và `"Ảnh Phụ`" trên form Chỉnh sửa Sản phẩm. Bật chế độ cho phép lưu mã HTML trong mô tả sản phẩm.`n")
$selection.Font.Italic = $true
$selection.Font.Color = 255 # Red
$selection.TypeText("  [Chèn ảnh: Chụp form Sửa Sản Phẩm hiện rõ Ảnh Chính và các Ảnh phụ được làm mờ bên dưới]`n`n")
$selection.Font.Italic = $false
$selection.Font.Color = 0

$selection.TypeText("- Sửa lỗi Phân loại khách hàng (Customer Segments): Xử lý lỗi RuntimeBinderException bằng cách truyền đúng Object CustomerSegmentVM từ Backend ra Frontend, giúp bảng Xếp hạng VIP hiển thị chuẩn xác.`n")
$selection.Font.Italic = $true
$selection.Font.Color = 255 # Red
$selection.TypeText("  [Chèn ảnh: Chụp giao diện trang Phân nhóm khách hàng với các mác VIP Kim Cương, Vàng...]`n`n")
$selection.Font.Italic = $false
$selection.Font.Color = 0

$selection.TypeText("- Tái cấu trúc Hệ thống Flash Sale: Can thiệp CSDL thêm thời hạn kết thúc (FlashSaleEndDate). Xây dựng trang `"Cập nhật Flash Sale Hàng Loạt`", cho phép gõ trực tiếp % hoặc số tiền giảm.`n")
$selection.Font.Italic = $true
$selection.Font.Color = 255 # Red
$selection.TypeText("  [Chèn ảnh: Chụp màn hình trang `"Cập Nhật Hàng Loạt`" (CreateFlashSale.cshtml) thấy rõ bảng nhập liệu giảm giá]`n`n")
$selection.Font.Italic = $false
$selection.Font.Color = 0

$selection.TypeText("- Tự động hóa giá Flash Sale: Viết code (ProductHelper) tự động kiểm tra và phục hồi lại Giá gốc ngoài trang chủ khi sản phẩm hết thời hạn giảm giá sốc.`n")
$selection.Font.Italic = $true
$selection.Font.Color = 255 # Red
$selection.TypeText("  [Chèn ảnh: Chụp giao diện trang Quản lý Flash Sale hiển thị danh sách các sản phẩm đang diễn ra kèm thời gian kết thúc]`n`n")
$selection.Font.Italic = $false
$selection.Font.Color = 0

$selection.Font.Bold = $true
$selection.TypeText("3. Kết quả đạt được:`n")
$selection.Font.Bold = $false
$selection.TypeText("Đến cuối tuần 9, trang Quản trị Admin đã hoạt động vô cùng trơn tru, không còn lỗi sập trang khi tương tác dữ liệu. Hệ thống Flash Sale mới là một bước tiến lớn, mang lại trải nghiệm thao tác đơn giản và tiện lợi hơn rất nhiều so với thiết kế cũ. Admin có thể tự do tung các đợt giảm giá có giới hạn thời gian một cách chuyên nghiệp.`n`n")

$selection.Font.Bold = $true
$selection.TypeText("4. Khó khăn và hướng xử lý:`n")
$selection.Font.Bold = $false
$selection.TypeText("- Khó khăn: Gặp khó khăn ban đầu khi lỗi Windows Security liên tục chặn việc biên dịch của ASP.NET, cũng như sự nhầm lẫn trong logic Flash Sale cũ (bị gộp chung với Mã giảm giá).`n")
$selection.TypeText("- Hướng xử lý: Đã nghiên cứu tài liệu IIS/ASP.NET để cấu hình lại vùng nhớ tạm an toàn (TempBuild). Đối với Flash Sale, đã mạnh dạn tái cấu trúc lại database và flow thay vì cố gắng sửa trên nền tảng cũ.`n`n")

$selection.Font.Bold = $true
$selection.TypeText("5. Kết luận:`n")
$selection.Font.Bold = $false
$selection.TypeText("Tuần 9 đã khép lại với sự hài lòng cao về mặt trải nghiệm dành cho Người quản trị (Admin). Các lỗi cốt lõi đã được khắc phục hoàn toàn. Với quy trình Flash Sale được chuyên nghiệp hóa, nền tảng PandoraWeb đã hoàn thiện hơn rất nhiều ở khía cạnh công cụ Marketing và sẵn sàng hỗ trợ đẩy mạnh doanh số khi vận hành thực tế.`n`n")

$selection.TypeText("Link: https://docs.google.com/spreadsheets/d/1883ET-MajLVA1eyZFIwByjCHXHZjkqqK9pXgpOd2hDI/edit?usp=sharing`n")

$path = "$pwd\Baocaotiendotuan9_NguyenTheAn_2400004657.docx"
$doc.SaveAs([ref]$path, [ref]16)
$doc.Close()
$word.Quit()
