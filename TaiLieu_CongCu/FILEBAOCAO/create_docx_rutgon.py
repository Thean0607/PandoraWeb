import docx
from docx.shared import Pt
from docx.enum.text import WD_PARAGRAPH_ALIGNMENT

doc = docx.Document()
doc.add_heading('CHƯƠNG 2: CƠ SỞ LÝ THUYẾT', level=1)

text_content = """
2.1. Nền tảng .NET Framework và Ngôn ngữ C#
2.1.1. Tổng quan về .NET Framework 4.7.2
[IMG]
.NET Framework 4.7.2 là nền tảng phát triển ứng dụng của Microsoft, cung cấp môi trường thực thi ổn định cho dự án PandoraWeb. Nền tảng bao gồm Thư viện lớp cơ sở (BCL) để xử lý các tác vụ đa dụng và Môi trường thực thi (CLR) giúp quản lý bộ nhớ tự động, đảm bảo an ninh hệ thống.

2.1.2. Ngôn ngữ lập trình C#
[IMG]
C# là ngôn ngữ lập trình hướng đối tượng (OOP) đóng vai trò xử lý toàn bộ logic nghiệp vụ phía Server. C# nổi bật với tính định kiểu chặt chẽ, hỗ trợ truy vấn LINQ an toàn và lập trình bất đồng bộ (Async/Await) giúp tối ưu hóa hiệu năng hệ thống.

2.1.3. Cơ chế biên dịch và quản lý bộ nhớ
Mã nguồn C# được biên dịch qua hai giai đoạn: sang mã trung gian (MSIL) và mã máy (JIT), đảm bảo tương thích phần cứng. Việc quản lý bộ nhớ được tự động hóa hoàn toàn nhờ bộ thu gom rác (Garbage Collection - GC), ngăn ngừa triệt để tình trạng rò rỉ vùng nhớ.

2.2. Mô hình kiến trúc ASP.NET MVC 5
2.2.1. Mô hình kiến trúc MVC
[IMG]
Kiến trúc MVC tách biệt hệ thống thành 3 phần: Model (Dữ liệu), View (Giao diện) và Controller (Điều khiển). Việc phân tách này giúp mã nguồn dễ bảo trì, dễ mở rộng và hỗ trợ nhóm phát triển làm việc song song hiệu quả.

2.2.2. Thành phần Model và ViewModel
Model đại diện cho dữ liệu cơ sở dữ liệu, ánh xạ trực tiếp từ SQL Server. ViewModel là lớp dữ liệu trung gian dành riêng cho View, giúp bảo mật cấu trúc CSDL gốc và tích hợp các ràng buộc kiểm tra dữ liệu đầu vào.

2.2.3. Thành phần View và Engine Razor
View hiển thị giao diện người dùng bằng HTML/CSS. Dự án sử dụng Razor Engine (.cshtml) để nhúng mã C# trực tiếp vào HTML, kết hợp với Layout và Partial Views để tái sử dụng các thành phần giao diện (như Header, Footer).

2.2.4. Thành phần Controller và Cơ chế Routing
Controller là trung tâm tiếp nhận yêu cầu (HTTP Request), xử lý logic và trả về View phù hợp. Hệ thống sử dụng Cơ chế Routing để tạo các đường dẫn URL thân thiện, tối ưu cho SEO.

2.2.5. Cơ chế xác thực và phân quyền
Hệ thống duy trì trạng thái đăng nhập bằng Session và HttpCookie. Các bộ lọc Action Filters (như [Authorize]) được sử dụng để kiểm tra quyền và phân quyền rõ ràng giữa khách hàng và Quản trị viên (Admin).

2.3. Công nghệ truy vấn và Quản trị Cơ sở dữ liệu
2.3.1. Hệ quản trị CSDL Microsoft SQL Server
[IMG]
SQL Server là hệ quản trị cơ sở dữ liệu quan hệ lưu trữ toàn bộ dữ liệu của PandoraWeb. Nó đảm bảo tính toàn vẹn dữ liệu (tiêu chuẩn ACID) và duy trì sự chính xác giữa các bảng qua hệ thống Khóa chính - Khóa ngoại.

2.3.2. Framework tương tác CSDL Entity Framework 6 (EF6)
[IMG]
EF6 là công cụ ORM tự động ánh xạ cơ sở dữ liệu thành đối tượng C#. Dự án sử dụng mô hình Code-First kết hợp tính năng theo dõi thay đổi (Change Tracking) để thao tác với CSDL dễ dàng mà không cần viết lệnh SQL thủ công.

2.3.3. Ngôn ngữ truy vấn LINQ
[IMG]
LINQ giúp viết các truy vấn lọc và sắp xếp dữ liệu ngắn gọn trực tiếp bằng C#. Mọi câu lệnh đều được dịch ngầm sang SQL và tự động tham số hóa, loại bỏ hoàn toàn nguy cơ tấn công SQL Injection.

2.4. Dịch vụ lưu trữ tài nguyên đám mây Cloudinary
[IMG]
Cloudinary là nền tảng quản lý hình ảnh trên đám mây. Dự án sử dụng dịch vụ này để tự động tải lên, nén dung lượng và phân phối ảnh sản phẩm qua mạng CDN, giúp website tải trang cực kỳ nhanh chóng.

2.5. Công nghệ phát triển Giao diện (Frontend)
2.5.1. HTML5 và CSS3
[IMG]
HTML5 cung cấp cấu trúc trang web chuẩn ngữ nghĩa (Semantic), hỗ trợ tốt cho SEO. Trong khi đó, CSS3 tạo phong cách thẩm mỹ hiện đại, màu sắc và các hiệu ứng chuyển động mượt mà cho giao diện website.

2.5.2. Framework Bootstrap 5
[IMG]
Bootstrap 5 cung cấp hệ thống lưới (Grid) linh hoạt giúp website tự động co giãn, thích ứng hoàn hảo trên mọi thiết bị (PC, Mobile). Các thành phần đúc sẵn (Navbar, Modal) giúp tiết kiệm đáng kể thời gian thiết kế UI.

2.5.3. Thư viện jQuery
[IMG]
jQuery xử lý tương tác động và gửi truy vấn ngầm (AJAX) mà không cần tải lại trang. Tính năng Client-side Validation của jQuery giúp kiểm tra lỗi form ngay tại trình duyệt, giảm tải xử lý cho máy chủ.

2.5.4. Thư viện Newtonsoft.Json
[IMG]
Newtonsoft.Json đảm nhiệm việc mã hóa và giải mã dữ liệu chuẩn JSON. Thư viện giúp luồng giao tiếp dữ liệu giữa Frontend và Backend thông qua API diễn ra nhanh chóng, chính xác và bảo mật.
"""

for line in text_content.strip().split('\n'):
    line = line.strip()
    if not line:
        continue
    
    if line.startswith('2.'):
        if len(line.split('.')) == 3: # 2.1. Nền tảng...
            doc.add_heading(line, level=2)
        elif len(line.split('.')) == 4: # 2.1.1. Tổng quan...
            doc.add_heading(line, level=3)
        else:
            doc.add_heading(line, level=3)
    elif line == '[IMG]':
        p_img = doc.add_paragraph()
        p_img.alignment = WD_PARAGRAPH_ALIGNMENT.CENTER
        run_img = p_img.add_run('[HÌNH ẢNH LOGO CÔNG NGHỆ BẠN CHÈN VÀO ĐÂY (Canh giữa)]')
        run_img.italic = True
        run_img.bold = True
    else:
        p = doc.add_paragraph(line)
        p.alignment = WD_PARAGRAPH_ALIGNMENT.JUSTIFY

# Apply font style
for p in doc.paragraphs:
    for run in p.runs:
        run.font.name = 'Times New Roman'
        run.font.size = Pt(13)

output_path = r'c:\Users\thean\Desktop\PandoraWeb\FILEBAOCAO\Chuong2_RutGon.docx'
doc.save(output_path)
print(f'Saved to {output_path}')
