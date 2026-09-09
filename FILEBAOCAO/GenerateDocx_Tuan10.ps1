$word = New-Object -ComObject Word.Application
$word.Visible = $false
$doc = $word.Documents.Add()
$selection = $word.Selection

$selection.Font.Name = "Times New Roman"
$selection.Font.Size = 14
$selection.Font.Bold = $true
$selection.TypeText("BÁO CÁO TIẾN ĐỘ TUẦN 10`n")

$selection.Font.Size = 13
$selection.Font.Bold = $false
$selection.TypeText("Họ và tên: Nguyễn Thế An`nMSSV: 2400004657`n`n")

$selection.Font.Bold = $true
$selection.TypeText("1. Tổng quan tiến độ tuần 10:`n")
$selection.Font.Bold = $false
$selection.TypeText("Trong tuần 10, dự án PandoraWeb tập trung vào việc hoàn thiện trải nghiệm giao diện người dùng và nâng cấp các tính năng cốt lõi cho cả Khách hàng lẫn Quản trị viên (Admin). Trọng tâm lớn nhất của tuần này là cấu hình và lập trình thành công hệ thống thanh toán trực tuyến qua cổng VNPAY (môi trường Sandbox). Ngoài ra, em cũng đã xây dựng tính năng kết xuất dữ liệu (Export Excel/CSV) trực tiếp từ giao diện Admin và xử lý triệt để các lỗi hiển thị UI quan trọng trên hệ thống.`n`n")

$selection.Font.Bold = $true
$selection.TypeText("2. Công việc đã thực hiện trong tuần 10:`n")
$selection.Font.Bold = $false
$selection.TypeText("Các đầu việc trong tuần 10 được triển khai bám sát yêu cầu tối ưu hóa và mở rộng chức năng:`n`n")

$selection.TypeText("- Tích hợp Thanh toán trực tuyến VNPAY Sandbox: Cấu hình Web.config để khai báo các khoá bảo mật. Tạo VnPayController và thư viện VnPayLibrary để mã hóa thông số, tạo URL thanh toán và xử lý IPN/Return khi giao dịch hoàn tất. Giao diện Checkout được bổ sung thêm phương thức thanh toán trực tuyến qua VNPAY.`n")
$selection.Font.Italic = $true
$selection.Font.Color = 255 # Red
$selection.TypeText("  [Chèn ảnh: Chụp màn hình trang Checkout có thêm nút Radio chọn Thanh toán VNPAY]`n`n")
$selection.Font.Italic = $false
$selection.Font.Color = 0

$selection.TypeText("- Thiết lập môi trường kiểm thử Localhost với Ngrok: Triển khai cài đặt và cấu hình Ngrok để public cổng localhost của Visual Studio ra Internet. Điều này giúp hệ thống VNPAY Sandbox có thể gọi hàm IPN tự động về máy cá nhân để cập nhật trạng thái đơn hàng ngay khi khách hàng chuyển tiền thành công.`n")
$selection.Font.Italic = $true
$selection.Font.Color = 255 # Red
$selection.TypeText("  [Chèn ảnh: Chụp màn hình Console Ngrok đang chạy với địa chỉ https forwarding và file vnpay_setup_guide.md]`n`n")
$selection.Font.Italic = $false
$selection.Font.Color = 0

$selection.TypeText("- Xây dựng tính năng Xuất dữ liệu báo cáo (Export CSV): Viết hàm Javascript cốt lõi (exportTableToCSV) để trích xuất dữ liệu từ các bảng HTML của trang Admin (Đơn hàng, Khách hàng) ra file .csv. Đặc biệt, đã mã hoá file với định dạng UTF-8 (có BOM) để đảm bảo mở trên Microsoft Excel không bị lỗi font Tiếng Việt.`n")
$selection.Font.Italic = $true
$selection.Font.Color = 255 # Red
$selection.TypeText("  [Chèn ảnh: Chụp trang Đơn hàng Admin có nút `"Xuất dữ liệu`" và file excel hiển thị chuẩn tiếng Việt]`n`n")
$selection.Font.Italic = $false
$selection.Font.Color = 0

$selection.TypeText("- Sửa lỗi hiển thị UI Chi tiết sản phẩm: Cấu trúc lại logic hiển thị sao đánh giá, khắc phục lỗi hệ thống tự động cho 5 sao dù sản phẩm có 0 lượt đánh giá (hiện tại hiển thị 5 sao rỗng). Sửa lỗi trùng màu chữ/màu nền ở các Tab (Mô tả sản phẩm, Thông tin chi tiết, Đánh giá) giúp người dùng dễ dàng nhìn thấy văn bản mà không cần phải nhấp chuột vào.`n")
$selection.Font.Italic = $true
$selection.Font.Color = 255 # Red
$selection.TypeText("  [Chèn ảnh: Chụp giao diện Sản phẩm hiển thị 0 đánh giá với sao rỗng và các Tab hiển thị chữ xám đậm rõ ràng]`n`n")
$selection.Font.Italic = $false
$selection.Font.Color = 0


$selection.Font.Bold = $true
$selection.TypeText("3. Kết quả đạt được:`n")
$selection.Font.Bold = $false
$selection.TypeText("Dự án đã tiến một bước lớn tới thực tế khi kết nối thành công với cổng thanh toán VNPAY. Mọi giao dịch thử nghiệm đều trả về trạng thái chuẩn xác thông qua Ngrok. Đồng thời, công cụ Export Excel đã giúp bảng Admin trở nên hữu ích và chuyên nghiệp hơn rất nhiều. Các lỗi hiển thị gây khó chịu ở trang Sản phẩm đã được giải quyết triệt để.`n`n")

$selection.Font.Bold = $true
$selection.TypeText("4. Khó khăn và hướng xử lý:`n")
$selection.Font.Bold = $false
$selection.TypeText("- Khó khăn: VNPAY yêu cầu phải có một tên miền thật (public URL) để nhận thông báo thanh toán (IPN) từ server của họ, điều này không thể làm được với địa chỉ http://localhost. Ngoài ra, tính năng xuất Excel gặp hiện tượng lỗi font Tiếng Việt khi sử dụng Javascript thuần.`n")
$selection.TypeText("- Hướng xử lý: Đã tìm hiểu và sử dụng công cụ Ngrok để tạo đường hầm (tunnel) kết nối ra Internet tạm thời cho localhost. Đối với file CSV, đã xử lý bằng cách gắn thêm mã BOM (\uFEFF) vào đầu file dữ liệu trước khi nén thành đối tượng Blob tải xuống, giúp Excel nhận diện chính xác font chữ.`n`n")

$selection.Font.Bold = $true
$selection.TypeText("5. Kết luận:`n")
$selection.Font.Bold = $false
$selection.TypeText("Tuần 10 đã bổ sung những mắt xích đặc biệt quan trọng (Thanh toán trực tuyến & Báo cáo dữ liệu) cho nền tảng PandoraWeb. Website đang có sự hoàn thiện rất cao và ổn định về mặt hệ thống. Trong tuần tiếp theo, em sẽ tập trung chốt lại các sơ đồ kiến trúc và Use Case, sẵn sàng cho công đoạn triển khai (Deploy) chính thức dự án.`n`n")

$selection.TypeText("Link: https://docs.google.com/spreadsheets/d/1883ET-MajLVA1eyZFIwByjCHXHZjkqqK9pXgpOd2hDI/edit?usp=sharing`n")

$path = "$pwd\Baocaotiendotuan10_NguyenTheAn_2400004657.docx"
$doc.SaveAs([ref]$path, [ref]16)
$doc.Close()
$word.Quit()
