import docx
from docx.shared import Pt
from docx.enum.text import WD_PARAGRAPH_ALIGNMENT
import io

doc = docx.Document()
doc.add_heading('CHƯƠNG 2: CƠ SỞ LÝ THUYẾT', level=1)

text_content = """
2.1. Nền tảng .NET Framework và Ngôn ngữ C#
2.1.1. Tổng quan về .NET Framework 4.7.2
[IMG]
.NET Framework là nền tảng lập trình và phát triển ứng dụng toàn diện do Microsoft nghiên cứu và phát triển. Phiên bản .NET Framework 4.7.2 được lựa chọn ứng dụng trong dự án PandoraWeb nhằm cung cấp môi trường thực thi hiện đại, ổn định, hỗ trợ tối đa cho việc xây dựng các ứng dụng web quy mô thương mại.
.NET Framework bao gồm hai thành phần cốt lõi:
•	Thư viện lớp cơ sở (Base Class Library - BCL): Cung cấp hệ thống các lớp (class) tích hợp sẵn hỗ trợ xử lý chuỗi, đọc/ghi tệp, tương tác cơ sở dữ liệu, xử lý mật mã, kết nối mạng và các cấu trúc dữ liệu nâng cao.
•	Môi trường thực thi (Common Language Runtime - CLR): Thành phần trung tâm của nền tảng .NET, đảm nhiệm vai trò quản lý quá trình thực thi ứng dụng, cung cấp các dịch vụ như quản lý bộ nhớ tự động, quản lý tiểu trình (thread), xử lý ngoại lệ và đảm bảo an ninh hệ thống.
2.1.2. Ngôn ngữ lập trình C#
[IMG]
C# là ngôn ngữ lập trình hướng đối tượng (OOP) hiện đại, được Microsoft thiết kế tối ưu cho nền tảng .NET. Trong hệ thống PandoraWeb, toàn bộ các xử lý nghiệp vụ phía Server-side (Controllers, Models, Helpers) đều được xây dựng bằng ngôn ngữ C#.
Các đặc tính kỹ thuật quan trọng của C# ứng dụng trong dự án:
•	Tính hướng đối tượng (Object-Oriented): Đáp ứng đầy đủ 4 nguyên lý cơ bản gồm Đóng gói (Encapsulation), Kế thừa (Inheritance), Đa hình (Polymorphism) và Trừu tượng (Abstraction). Các thực thể hệ thống như Product, Order, Customer, Account được mô hình hóa thành các lớp đối tượng minh bạch.
•	Kiểu dữ liệu chặt chẽ (Strongly Typed): Hỗ trợ kiểm tra và phát hiện lỗi dữ liệu ngay tại thời điểm biên dịch (Compile-time), giảm thiểu nguy cơ phát sinh lỗi trong quá trình thực thi (Runtime).
•	Tích hợp LINQ (Language Integrated Query): Cho phép truy vấn dữ liệu trực tiếp trong cú pháp C#, đảm bảo mã nguồn ngắn gọn và an toàn về kiểu dữ liệu.
•	Lập trình bất đồng bộ (Async/Await): Hỗ trợ tối ưu hóa hiệu năng xử lý của máy chủ đối với các tác vụ vào/ra (I/O Bound) như truy xuất cơ sở dữ liệu hoặc gọi các dịch vụ API bên ngoài (Cloudinary API).
2.1.3. Cơ chế biên dịch và quản lý bộ nhớ
Quá trình biên dịch mã nguồn trong .NET được thực hiện qua hai giai đoạn chính:
1.	Biên dịch mã nguồn: Trình biên dịch C# chuyển đổi mã nguồn thành mã trung gian MSIL (Microsoft Intermediate Language), đảm bảo tính độc lập với phần cứng.
2.	Biên dịch JIT (Just-In-Time): Khi ứng dụng vận hành, trình biên dịch JIT của CLR tiếp tục dịch mã MSIL thành mã máy (Native Code) tương thích chính xác với hệ điều hành và cấu hình phần cứng hiện hành.
Cơ chế thu gom rác tự động (Garbage Collection - GC): CLR đảm nhiệm việc quản lý vùng nhớ Heap thông qua cơ chế Garbage Collection. Bộ thu gom rác sẽ định kỳ rà soát, xác định và giải phóng vùng nhớ của các đối tượng không còn được tham chiếu, ngăn ngừa hiệu quả hiện tượng rò rỉ bộ nhớ (Memory Leak) hoặc truy cập con trỏ không hợp lệ.
2.2. Mô hình kiến trúc ASP.NET MVC 5
2.2.1. Mô hình kiến trúc MVC
[IMG]
Mô hình MVC (Model - View - Controller) là mẫu kiến trúc phần mềm chuẩn trong phát triển ứng dụng web.
Mục đích kiến trúc: Tách biệt rõ ràng giữa logic giao diện (UI), logic nghiệp vụ (Business Logic) và logic truy xuất dữ liệu (Data Access), tuân thủ nguyên lý Separation of Concerns (SoC). Kiến trúc này giúp nâng cao tính bảo trì, khả năng mở rộng và thuận tiện cho việc kiểm thử phần mềm.
2.2.2. Thành phần Model và ViewModel
•	Model: Đại diện cho dữ liệu và quy tắc nghiệp vụ của hệ thống. Trong PandoraWeb, Model bao gồm các lớp Entity ánh xạ tương ứng với các bảng trong CSDL SQL Server (như KhachHang, SanPham, DonHang, ChiTietDonHang).
•	ViewModel: Đóng vai trò là các lớp dữ liệu trung gian được cấu trúc riêng cho từng View cụ thể, mang lại các ưu điểm:
o	Tách biệt cấu trúc dữ liệu cơ sở dữ liệu với dữ liệu hiển thị trên giao diện.
o	Tích hợp các quy tắc ràng buộc dữ liệu ([Required], [StringLength], [Compare]) thông qua DataAnnotations.
o	Đóng gói dữ liệu tổng hợp từ nhiều thực thể mà không làm rò rỉ cấu trúc CSDL nhạy cảm ra ngoài giao diện.
2.2.3. Thành phần View và Engine Razor
•	View: Thành phần đảm nhiệm vai trò trực quan hóa dữ liệu gửi từ Server thành giao diện hiển thị HTML/CSS cho người dùng.
•	Razor View Engine: Cú pháp sinh HTML phía Server (.cshtml), cho phép kết hợp linh hoạt giữa thẻ HTML tiêu chuẩn và mã C# thông qua ký tự @.
•	Layout Pages (_Layout.cshtml): Định nghĩa khung giao diện dùng chung (Header, Navigation, Footer) cho toàn bộ hệ thống, đảm bảo tính nhất quán về mặt giao diện và tuân thủ nguyên lý DRY (Don't Repeat Yourself).
•	Partial Views: Các thành phần giao diện mô-đun hóa nhỏ (như giỏ hàng nhanh, danh mục sản phẩm, thanh tìm kiếm), cho phép nhúng tái sử dụng tại nhiều trang khác nhau.
2.2.4. Thành phần Controller và Cơ chế Routing
•	Controller: Thành phần điều phối trung tâm của ứng dụng. Controller tiếp nhận HTTP Request từ Client, gọi các hàm xử lý nghiệp vụ ở Model, sau đó chọn View tương ứng để trả kết quả về cho người dùng.
•	Cơ chế Routing (Định tuyến URL): ASP.NET MVC sử dụng bảng định tuyến (RouteTable) để ánh xạ các đường dẫn URL thân thiện tới Controller và Action tương ứng.
2.2.5. Cơ chế xác thực và phân quyền
•	Session State: Lưu trữ thông tin phiên làm việc của người dùng phía Server
•	HttpCookie: Lưu giữ chuỗi định danh tại trình duyệt Client nhằm phục vụ tính năng duy trì trạng thái đăng nhập.
•	Action Filters ([Authorize], Custom Filters): Can thiệp vào quy trình xử lý Request để kiểm tra quyền truy cập trước khi thực thi Action, hỗ trợ phân quyền giữa tài khoản người dùng thông thường và tài khoản Quản trị (Admin).
2.3. Công nghệ truy vấn và Quản trị Cơ sở dữ liệu
2.3.1. Hệ quản trị cơ sở dữ liệu Microsoft SQL Server
[IMG]
Microsoft SQL Server là hệ quản trị cơ sở dữ liệu quan hệ (RDBMS) cung cấp khả năng lưu trữ, truy xuất dữ liệu an toàn và hiệu năng cao.
Các thuộc tính cốt lõi được áp dụng trong dự án PandoraWeb:
•	Đảm bảo tính toàn vẹn dữ liệu (Tiêu chuẩn ACID): Đảm bảo mọi giao dịch đơn hàng (Transaction) đều thỏa mãn tính Nguyên tố (Atomicity), Nhất quán (Consistency), Độc lập (Isolation) và Bền vững (Durability).
•	Ràng buộc quan hệ (Primary Key / Foreign Key): Duy trì tính chính xác và toàn vẹn tham chiếu giữa các thực thể dữ liệu (như Đơn hàng - Chi tiết đơn hàng, Sản phẩm - Danh mục).
2.3.2. Framework tương tác CSDL Entity Framework 6 (EF6)
[IMG]
Entity Framework 6 là công nghệ ORM (Object-Relational Mapping) mã nguồn mở cho .NET, cho phép lập trình viên thao tác với cơ sở dữ liệu quan hệ thông qua các đối tượng C# thuần túy.
•	DbContext & DbSet: Lớp DbContext đại diện cho một phiên làm việc với CSDL. Mỗi bảng dữ liệu được ánh xạ tương ứng thành một tập hợp DbSet<TEntity>.
•	Phương pháp tiếp cận: Hỗ trợ linh hoạt các mô hình triển khai như Code First (định nghĩa các lớp C# trước để tự động tạo CSDL) hoặc Database First (thiết kế CSDL trước rồi ánh xạ thành Model C#).
•	Theo dõi thay đổi (Change Tracking): EF6 tự động ghi nhận các trạng thái thay đổi của đối tượng (Thêm, Sửa, Xóa) và tự động sinh các câu lệnh SQL (INSERT, UPDATE, DELETE) tương ứng khi phương thức SaveChanges() được gọi.
2.3.3. Ngôn ngữ truy vấn LINQ (Language Integrated Query)
[IMG]
LINQ cung cấp khả năng viết các truy vấn dữ liệu trực tiếp trong C# với cú pháp rõ ràng và an toàn về kiểu dữ liệu.
•	LINQ to Entities: Biên dịch các biểu thức LINQ trong C# thành các câu lệnh SQL tối ưu để thực thi trực tiếp trên hệ quản trị SQL Server.
•	Cú pháp Method Syntax / Query Syntax: Hỗ trợ các phương thức lọc và sắp xếp dữ liệu như .Where(), .Select(), .OrderBy(), .FirstOrDefault(), và .Include() (Eager Loading dữ liệu liên quan).
•	An toàn bảo mật: Tự động áp dụng cơ chế truy vấn tham số hóa (Parameterized Queries), loại bỏ hoàn toàn các nguy cơ tấn công theo dạng SQL Injection.
2.4. Dịch vụ lưu trữ tài nguyên đám mây Cloudinary API
2.4.1. Giới thiệu về Cloudinary và CloudinaryDotNet SDK
[IMG]
Cloudinary là nền tảng quản lý truyền thông đám mây (PaaS) cung cấp giải pháp lưu trữ, tối ưu hóa và phân phối tài nguyên hình ảnh/video. Trong hệ thống PandoraWeb, thư viện CloudinaryDotNet SDK được tích hợp trực tiếp vào ứng dụng Server-side C#.
2.4.2. Cơ chế Upload và tối ưu hóa hình ảnh
•	Lưu trữ đám mây (Cloud Storage): Khi người quản trị thực hiện thêm hoặc cập nhật hình ảnh sản phẩm, tệp tin phương tiện thay vì lưu cục bộ trên máy chủ web sẽ được tải trực tiếp lên hạ tầng đám mây của Cloudinary thông qua Cloudinary API.
•	Phân phối qua mạng CDN: Cloudinary cung cấp đường dẫn URL tuyệt đối và phân phối hình ảnh thông qua mạng lưới CDN toàn cầu, giúp giảm thiểu tải trọng bộ nhớ và băng thông cho Web Server.
•	Tối ưu hóa tự động: Tự động thực hiện nén dung lượng, căn chỉnh kích thước (Crop/Resize) và chuyển đổi định dạng tối ưu (WebP/JPEG), hỗ trợ tăng tốc độ tải trang sản phẩm.
2.5. Công nghệ phát triển Giao diện người dùng (Frontend)
2.5.1. HTML5 và CSS3
[IMG]
•	HTML5: Cung cấp cấu trúc trang web chuẩn ngữ nghĩa thông qua các thẻ Semantic (như <header>, <nav>, <main>, <article>, <footer>, <section>), hỗ trợ tối ưu hóa công cụ tìm kiếm (SEO) và tăng khả năng truy cập.
•	CSS3: Đảm nhiệm việc định hình phong cách giao diện thẩm mỹ cho website (sử dụng hiệu ứng Glassmorphism, phối màu hệ thống, hiệu ứng chuyển cảnh transitions và animations mượt mà).
2.5.2. Framework Bootstrap 5
[IMG]
Bootstrap 5 là framework CSS mã nguồn mở hỗ trợ thiết kế giao diện thích ứng (Responsive Web Design).
•	Grid System: Hệ thống lưới 12 cột dựa trên Flexbox giúp giao diện PandoraWeb tự động co giãn và hiển thị tối ưu trên các loại màn hình (Desktop, Tablet, Mobile).
•	UI Components: Sử dụng các thành phần giao diện chuẩn hóa như Navbar, Cards, Modal, Dropdown, Carousel và Badges nhằm nâng cao trải nghiệm người dùng (UX).
2.5.3. Thư viện jQuery và Client-side Validation
[IMG]
•	jQuery (v3.7.0): Thư viện JavaScript hỗ trợ tối giản các thao tác xử lý cây DOM, quản lý sự kiện và thực hiện các truy vấn AJAX không tải lại trang (như cập nhật nhanh giỏ hàng hoặc lọc sản phẩm).
•	jQuery Validation & Unobtrusive Validation: Thực hiện kiểm tra tính hợp lệ của dữ liệu đầu vào ngay tại phía trình duyệt Client trước khi gửi Request về Server, góp phần giảm tải xử lý cho máy chủ và cải thiện trải nghiệm người dùng.
2.5.4. Thư viện Newtonsoft.Json (Json.NET)
[IMG]
Newtonsoft.Json là thư viện xử lý dữ liệu JSON tiêu chuẩn trong nền tảng .NET, đảm nhiệm vai trò tuần tự hóa và giải tuần tự hóa (Serialize/Deserialize) giữa các đối tượng C# và chuỗi định dạng JSON khi trao đổi dữ liệu qua API hoặc các truy vấn AJAX.
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

output_path = r'c:\Users\thean\Desktop\PandoraWeb\FILEBAOCAO\Chuong2_GiuNguyenText_ThemAnh.docx'
doc.save(output_path)
print(f'Saved to {output_path}')
