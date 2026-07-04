using System;
using System.IO;

class Program {
    static void Main() {
        string[] files = Directory.GetFiles(@"c:\Users\thean\Desktop\Đồ án cơ sở\PandoraWeb\Views", "*.cshtml", SearchOption.AllDirectories);
        foreach(var path in files) {
            string text = File.ReadAllText(path, System.Text.Encoding.UTF8);
            if(text.Contains("Ã")) {
                text = text.Replace("Chi Tiáº¿t Sáº£n Pháº©m", "Chi Tiết Sản Phẩm")
                           .Replace("Sáº£n Pháº©m", "Sản Phẩm")
                           .Replace("Ä‘Ã¡nh giÃ¡", "đánh giá")
                           .Replace("â‚«", "₫")
                           .Replace("Háº¿t hÃ ng", "Hết hàng")
                           .Replace("HÆ°á»›ng dáº«n chá» n size", "Hướng dẫn chọn size")
                           .Replace("Miá»…n phÃ­ váº­n chuyá»ƒn toÃ n quá»‘c", "Miễn phí vận chuyển toàn quốc")
                           .Replace("Báº£o hÃ nh trá» n Ä‘á» i", "Bảo hành trọn đời")
                           .Replace("Ä á»•i tráº£ trong 15 ngÃ y", "Đổi trả trong 15 ngày")
                           .Replace("MÃ´ Táº£", "Mô Tả")
                           .Replace("ThÃ´ng Tin", "Thông Tin")
                           .Replace("Má»—i chi tiáº¿t", "Mỗi chi tiết")
                           .Replace("trÃªn chiáº¿c nháº«n Ä‘á» u Ä‘Æ°á»£c", "trên chiếc nhẫn đều được")
                           .Replace("cháº¿ tÃ¡c thá»§ cÃ´ng bá»Ÿi nhá»¯ng nghá»‡ nhÃ¢n", "chế tác thủ công bởi những nghệ nhân")
                           .Replace("lÃ nh nghá»  nháº¥t", "lành nghề nhất")
                           .Replace("ViÃªn kim cÆ°Æ¡ng chá»§ Ä‘Æ°á»£c tuyá»ƒn chá» n ká»¹ lÆ°á»¡ng", "Viên kim cương chủ được tuyển chọn kỹ lưỡng")
                           .Replace("vá»›i Ä‘á»™ sÃ¡ng trong suá»‘t", "với độ sáng trong suốt")
                           .Replace("Ä‘Æ°á»£c nÃ¢ng Ä‘á»¡ bá»Ÿi nhá»¯ng cháº¥u vÃ ng tinh táº¿", "được nâng đỡ bởi những chấu vàng tinh tế")
                           .Replace("cho phÃ©p Ã¡nh sÃ¡ng xuyÃªn qua vÃ  pháº£n chiáº¿u rá»±c rá»¡", "cho phép ánh sáng xuyên qua và phản chiếu rực rỡ")
                           .Replace("tá»« má» i gÃ³c nhÃ¬n", "từ mọi góc nhìn")
                           .Replace("Cháº¥t liá»‡u", "Chất liệu")
                           .Replace("VÃ ng tráº¯ng 18K", "Vàng trắng 18K")
                           .Replace("Ä Ã¡ chÃ­nh", "Đá chính")
                           .Replace("Kim cÆ°Æ¡ng tá»± nhiÃªn", "Kim cương tự nhiên")
                           .Replace("Trá» ng lÆ°á»£ng", "Trọng lượng")
                           .Replace("chá»‰", "chỉ")
                           .Replace("Táº¥t Cáº£", "Tất Cả")
                           .Replace("Hiá»ƒn thá»‹ bá»™ lá» c", "Hiển thị bộ lọc")
                           .Replace("Lá» c", "Lọc")
                           .Replace("DÃ¢y Chuyá» n", "Dây Chuyền")
                           .Replace("Má»©c GiÃ¡", "Mức Giá")
                           .Replace("Ã p Dá»¥ng TÃ¹y Chá» n", "Áp Dụng Tùy Chọn")
                           .Replace("Hiá»ƒn thá»‹", "Hiển thị")
                           .Replace("trong sá»‘", "trong số")
                           .Replace("Má»›i nháº¥t", "Mới nhất")
                           .Replace("GiÃ¡: Tháº¥p Ä‘áº¿n Cao", "Giá: Thấp đến Cao")
                           .Replace("GiÃ¡: Cao xuá»‘ng Tháº¥p", "Giá: Cao xuống Thấp")
                           .Replace("Phá»• biáº¿n nháº¥t", "Phổ biến nhất")
                           .Replace("KhÃ´ng tÃ¬m tháº¥y", "Không tìm thấy")
                           .Replace("TÃ¬m cá»­a hÃ ng", "Tìm cửa hàng")
                           .Replace("gáº§n báº¡n nháº¥t Ä‘á»ƒ tráº£i nghiá»‡m vÃ  mua sáº¯m trá»±c tiáº¿p", "gần bạn nhất để trải nghiệm và mua sắm trực tiếp")
                           .Replace("Nháº­p tá»‰nh/thÃ nh phá»‘ hoáº·c quáº­n/huyá»‡n", "Nhập tỉnh/thành phố hoặc quận/huyện")
                           .Replace("TÃ¬m kiáº¿m", "Tìm kiếm")
                           .Replace("Táº§ng trá»‡t", "Tầng trệt")
                           .Replace("LÃª ThÃ¡nh TÃ´n", "Lê Thánh Tôn")
                           .Replace("PhÆ°á» ng Báº¿n NghÃ©", "Phường Bến Nghé")
                           .Replace("Quáº­n", "Quận")
                           .Replace("Há»“ ChÃ­ Minh", "Hồ Chí Minh")
                           .Replace("Thá»© 2 - Chá»§ Nháº­t", "Thứ 2 - Chủ Nhật")
                           .Replace("Chá»‰ Ä‘Æ°á» ng", "Chỉ đường")
                           .Replace("LÃª Lá»£i", "Lê Lợi")
                           .Replace("TÃ´n Dáº­t TiÃªn", "Tôn Dật Tiên")
                           .Replace("TÃ¢n PhÃº", "Tân Phú")
                           .Replace("BÃ  Triá»‡u", "Bà Triệu")
                           .Replace("LÃª Ä áº¡i HÃ nh", "Lê Đại Hành")
                           .Replace("Hai BÃ  TrÆ°ng", "Hai Bà Trưng")
                           .Replace("HÃ  Ná»™i", "Hà Nội")
                           .Replace("Miá»…n PhÃ­ Váº­n Chuyá»ƒn", "Miễn Phí Vận Chuyển")
                           .Replace("Cho má» i Ä‘Æ¡n hÃ ng trÃªn", "Cho mọi đơn hàng trên")
                           .Replace("KhÃ¡m phÃ¡", "Khám phá")
                           .Replace("Nháº«n", "Nhẫn")
                           .Replace("Hoa Tai", "Hoa Tai")
                           .Replace("Xem chi tiáº¿t", "Xem chi tiết")
                           .Replace("ThÃªm vÃ o giá»  hÃ ng", "Thêm vào giỏ hàng")
                           .Replace("YÃªu thÃ­ch", "Yêu thích")
                           .Replace("Danh Má»¥c Ná»•i Báº­t", "Danh Mục Nổi Bật")
                           .Replace("ChÆ°a cÃ³ sáº£n pháº©m nÃ o Ä‘Æ°á»£c hiá»ƒn thá»‹.", "Chưa có sản phẩm nào được hiển thị.")
                           .Replace("bá»™ sÆ°u táº­p mÃ¹a xuÃ¢n má»›i nháº¥t vá»›i cÃ¡c thiáº¿t káº¿ tinh xáº£o, tÃ´n vinh váº» Ä‘áº¹p vÄ©nh cá»­u.", "bộ sưu tập mùa xuân mới nhất với các thiết kế tinh xảo, tôn vinh vẻ đẹp vĩnh cửu.");
                
                File.WriteAllText(path, text, System.Text.Encoding.UTF8);
                Console.WriteLine("Fixed " + path);
            }
        }
    }
}
