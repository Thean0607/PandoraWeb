using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using PandoraWeb.Models;

namespace PandoraWeb.Helpers
{
    public static class EmailHelper
    {
        public static bool SendOrderConfirmationEmail(Order order, string recipientEmail, string recipientName)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail) || order == null)
                return false;

            try
            {
                string host = ConfigurationManager.AppSettings["Smtp:Host"] ?? "smtp.gmail.com";
                int port = int.TryParse(ConfigurationManager.AppSettings["Smtp:Port"], out int p) ? p : 587;
                bool enableSsl = bool.TryParse(ConfigurationManager.AppSettings["Smtp:EnableSsl"], out bool ssl) ? ssl : true;
                string username = ConfigurationManager.AppSettings["Smtp:Username"] ?? "";
                string password = (ConfigurationManager.AppSettings["Smtp:Password"] ?? "").Replace(" ", "");
                string senderName = ConfigurationManager.AppSettings["Smtp:SenderName"] ?? "PANDORA Trang Sức";

                string subject = $"[PANDORA] Xác nhận đơn hàng thành công #PAN{order.OrderId}";
                string body = BuildOrderEmailBody(order, recipientName);

                // Save local preview copy to ~/uploads/emails/ for verification
                SaveLocalEmailCopy(order.OrderId, recipientEmail, subject, body);

                // Send real email via SMTP if credentials are configured
                if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password) && username != "your_email@gmail.com")
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            using (MailMessage mail = new MailMessage())
                            {
                                mail.From = new MailAddress(username, senderName, Encoding.UTF8);
                                mail.To.Add(new MailAddress(recipientEmail.Trim(), recipientName));
                                mail.Subject = subject;
                                mail.Body = body;
                                mail.IsBodyHtml = true;
                                mail.BodyEncoding = Encoding.UTF8;
                                mail.SubjectEncoding = Encoding.UTF8;

                                using (SmtpClient smtp = new SmtpClient(host, port))
                                {
                                    smtp.Credentials = new NetworkCredential(username, password);
                                    smtp.EnableSsl = enableSsl;
                                    smtp.Send(mail);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("SMTP Exception: " + ex.Message);
                        }
                    });
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SendOrderConfirmationEmail error: " + ex.Message);
                return false;
            }
        }

        private static void SaveLocalEmailCopy(int orderId, string recipientEmail, string subject, string body)
        {
            try
            {
                if (HttpContext.Current != null)
                {
                    string folder = HttpContext.Current.Server.MapPath("~/uploads/emails/");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    string fileName = $"Order_PAN{orderId}_{DateTime.Now:yyyyMMdd_HHmmss}.html";
                    string filePath = Path.Combine(folder, fileName);

                    string header = $"<!-- To: {recipientEmail} | Subject: {subject} | Time: {DateTime.Now} -->\n";
                    File.WriteAllText(filePath, header + body, Encoding.UTF8);
                }
            }
            catch
            {
            }
        }

        private static string BuildOrderEmailBody(Order order, string recipientName)
        {
            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><head><meta charset='utf-8'></head><body style='font-family: Arial, sans-serif; background-color: #f8f9fa; margin: 0; padding: 20px;'>");
            sb.Append("<div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 30px; border-radius: 8px; border: 1px solid #eeeeee;'>");
            
            // Header
            sb.Append("<div style='text-align: center; border-bottom: 2px solid #d4af37; padding-bottom: 15px; margin-bottom: 20px;'>");
            sb.Append("<h1 style='color: #000; font-family: Georgia, serif; margin: 0; letter-spacing: 2px;'>PANDORA</h1>");
            sb.Append("<p style='color: #888; margin: 5px 0 0 0; font-size: 14px;'>XÁC NHẬN ĐƠN HÀNG THÀNH CÔNG</p>");
            sb.Append("</div>");

            // Greeting
            sb.Append($"<p>Xin chào <strong>{HttpUtility.HtmlEncode(recipientName)}</strong>,</p>");
            sb.Append("<p>Cảm ơn bạn đã lựa chọn trang sức Pandora! Đơn hàng của bạn đã được tiếp nhận thành công và đang trong quá trình xử lý.</p>");

            // Order Info Box
            sb.Append("<div style='background-color: #fdfbf7; border: 1px solid #f3e9d2; border-radius: 6px; padding: 15px; margin: 20px 0;'>");
            sb.Append($"<p style='margin: 5px 0;'><strong>Mã đơn hàng:</strong> <span style='color: #d4af37;'>#PAN{order.OrderId}</span></p>");
            sb.Append($"<p style='margin: 5px 0;'><strong>Thời gian đặt:</strong> {order.OrderDate:dd/MM/yyyy HH:mm}</p>");
            sb.Append($"<p style='margin: 5px 0;'><strong>Phương thức thanh toán:</strong> {(order.PaymentMethod == "BANK" ? "Chuyển khoản ngân hàng" : "Thanh toán khi nhận hàng (COD)")}</p>");
            if (order.ShippingAddress != null)
            {
                sb.Append($"<p style='margin: 5px 0;'><strong>Người nhận:</strong> {HttpUtility.HtmlEncode(order.ShippingAddress.ReceiverName)} ({HttpUtility.HtmlEncode(order.ShippingAddress.PhoneNumber)})</p>");
                sb.Append($"<p style='margin: 5px 0;'><strong>Địa chỉ giao:</strong> {HttpUtility.HtmlEncode(order.ShippingAddress.StreetAddress)}</p>");
            }
            sb.Append("</div>");

            // Products Table
            if (order.OrderItems != null && order.OrderItems.Count > 0)
            {
                sb.Append("<h3 style='font-family: Georgia, serif; color: #333; margin-top: 25px; margin-bottom: 10px;'>Chi Tiết Đơn Hàng</h3>");
                sb.Append("<table style='width: 100%; border-collapse: collapse;'>");
                sb.Append("<thead><tr style='background-color: #f8f9fa; text-align: left;'>");
                sb.Append("<th style='padding: 10px; border-bottom: 1px solid #ddd;'>Sản phẩm</th>");
                sb.Append("<th style='padding: 10px; border-bottom: 1px solid #ddd; text-align: center;'>Số lượng</th>");
                sb.Append("<th style='padding: 10px; border-bottom: 1px solid #ddd; text-align: right;'>Thành tiền</th>");
                sb.Append("</tr></thead><tbody>");

                foreach (var item in order.OrderItems)
                {
                    string productName = item.Variant?.Product?.ProductName ?? "Sản phẩm Pandora";
                    sb.Append("<tr>");
                    sb.Append($"<td style='padding: 10px; border-bottom: 1px solid #eee;'>{HttpUtility.HtmlEncode(productName)}</td>");
                    sb.Append($"<td style='padding: 10px; border-bottom: 1px solid #eee; text-align: center;'>{item.Quantity}</td>");
                    sb.Append($"<td style='padding: 10px; border-bottom: 1px solid #eee; text-align: right;'>{(item.Quantity * item.UnitPrice):N0} ₫</td>");
                    sb.Append("</tr>");
                }

                sb.Append("</tbody></table>");
            }

            // Total
            sb.Append("<div style='text-align: right; margin-top: 15px; font-size: 16px;'>");
            sb.Append($"<p><strong>Tổng cộng: <span style='color: #d4af37; font-size: 20px;'>{order.TotalAmount:N0} ₫</span></strong></p>");
            sb.Append("</div>");

            // Footer
            sb.Append("<div style='margin-top: 30px; padding-top: 15px; border-top: 1px solid #eee; text-align: center; color: #777; font-size: 12px;'>");
            sb.Append("<p style='margin: 5px 0;'>Nếu bạn có bất kỳ thắc mắc nào, vui lòng liên hệ hotline: <strong>1900 1234</strong> hoặc email: support@pandora.vn</p>");
            sb.Append("<p style='margin: 5px 0;'>PANDORA Jewelry © 2026 - Mọi quyền được bảo lưu.</p>");
            sb.Append("</div>");

            sb.Append("</div></body></html>");
            return sb.ToString();
        }

        public static bool SendPasswordResetEmail(string recipientEmail, string recipientName)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail))
                return false;

            try
            {
                string host = ConfigurationManager.AppSettings["Smtp:Host"] ?? "smtp.gmail.com";
                int port = int.TryParse(ConfigurationManager.AppSettings["Smtp:Port"], out int p) ? p : 587;
                bool enableSsl = bool.TryParse(ConfigurationManager.AppSettings["Smtp:EnableSsl"], out bool ssl) ? ssl : true;
                string username = ConfigurationManager.AppSettings["Smtp:Username"] ?? "";
                string password = (ConfigurationManager.AppSettings["Smtp:Password"] ?? "").Replace(" ", "");
                string senderName = ConfigurationManager.AppSettings["Smtp:SenderName"] ?? "PANDORA Trang Sức";

                string subject = "[PANDORA] Thông báo thay đổi mật khẩu thành công";
                string body = $@"<!DOCTYPE html><html><head><meta charset='utf-8'></head><body style='font-family: Arial, sans-serif; background-color: #f8f9fa; margin: 0; padding: 20px;'>
<div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 30px; border-radius: 8px; border: 1px solid #eeeeee;'>
<div style='text-align: center; border-bottom: 2px solid #d4af37; padding-bottom: 15px; margin-bottom: 20px;'>
<h1 style='color: #000; font-family: Georgia, serif; margin: 0; letter-spacing: 2px;'>PANDORA</h1>
<p style='color: #888; margin: 5px 0 0 0; font-size: 14px;'>THÔNG BÁO TÀI KHOẢN</p>
</div>
<p>Xin chào <strong>{HttpUtility.HtmlEncode(recipientName)}</strong>,</p>
<p>Mật khẩu cho tài khoản <strong>{HttpUtility.HtmlEncode(recipientEmail)}</strong> tại website <strong>PANDORA Trang Sức</strong> vừa được thay đổi thành công vào lúc {DateTime.Now:HH:mm dd/MM/yyyy}.</p>
<p style='background-color: #fff3cd; color: #856404; padding: 12px; border-radius: 5px; border-left: 4px solid #ffeba6;'>
⚠️ Nếu bạn không thực hiện yêu cầu này, vui lòng liên hệ ngay bộ phận hỗ trợ khách hàng Pandora qua hotline <strong>1900 1234</strong> để bảo vệ tài khoản.
</p>
<div style='margin-top: 30px; padding-top: 15px; border-top: 1px solid #eee; text-align: center; color: #777; font-size: 12px;'>
<p style='margin: 5px 0;'>PANDORA Jewelry © {DateTime.Now.Year} - Mọi quyền được bảo lưu.</p>
</div>
</div></body></html>";

                if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password) && username != "your_email@gmail.com")
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            using (MailMessage mail = new MailMessage())
                            {
                                mail.From = new MailAddress(username, senderName, Encoding.UTF8);
                                mail.To.Add(new MailAddress(recipientEmail.Trim(), recipientName));
                                mail.Subject = subject;
                                mail.Body = body;
                                mail.IsBodyHtml = true;
                                mail.BodyEncoding = Encoding.UTF8;
                                mail.SubjectEncoding = Encoding.UTF8;

                                using (SmtpClient smtp = new SmtpClient(host, port))
                                {
                                    smtp.Credentials = new NetworkCredential(username, password);
                                    smtp.EnableSsl = enableSsl;
                                    smtp.Send(mail);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("SMTP PasswordReset Exception: " + ex.Message);
                        }
                    });
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SendPasswordResetEmail error: " + ex.Message);
                return false;
            }
        }

        public static bool SendOtpEmail(string recipientEmail, string recipientName, string otpCode)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail) || string.IsNullOrWhiteSpace(otpCode))
                return false;

            try
            {
                string host = ConfigurationManager.AppSettings["Smtp:Host"] ?? "smtp.gmail.com";
                int port = int.TryParse(ConfigurationManager.AppSettings["Smtp:Port"], out int p) ? p : 587;
                bool enableSsl = bool.TryParse(ConfigurationManager.AppSettings["Smtp:EnableSsl"], out bool ssl) ? ssl : true;
                string username = ConfigurationManager.AppSettings["Smtp:Username"] ?? "";
                string password = (ConfigurationManager.AppSettings["Smtp:Password"] ?? "").Replace(" ", "");
                string senderName = ConfigurationManager.AppSettings["Smtp:SenderName"] ?? "PANDORA Trang Sức";

                string subject = $"[PANDORA] Mã xác thực OTP đặt lại mật khẩu: {otpCode}";
                string body = $@"<!DOCTYPE html><html><head><meta charset='utf-8'></head><body style='font-family: Arial, sans-serif; background-color: #f8f9fa; margin: 0; padding: 20px;'>
<div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 30px; border-radius: 8px; border: 1px solid #eeeeee;'>
<div style='text-align: center; border-bottom: 2px solid #d4af37; padding-bottom: 15px; margin-bottom: 20px;'>
<h1 style='color: #000; font-family: Georgia, serif; margin: 0; letter-spacing: 2px;'>PANDORA</h1>
<p style='color: #888; margin: 5px 0 0 0; font-size: 14px;'>MÃ XÁC THỰC ĐẶT LẠI MẬT KHẨU</p>
</div>
<p>Xin chào <strong>{HttpUtility.HtmlEncode(recipientName)}</strong>,</p>
<p>Bạn đã gửi yêu cầu đặt lại mật khẩu cho tài khoản <strong>{HttpUtility.HtmlEncode(recipientEmail)}</strong> tại Pandora Trang Sức.</p>
<p style='margin-top: 20px; font-weight: bold;'>Mã xác thực OTP 6 số của bạn là:</p>
<div style='text-align: center; margin: 25px 0;'>
<span style='font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #d4af37; background-color: #fdfbf7; padding: 12px 30px; border: 2px dashed #d4af37; border-radius: 8px; display: inline-block;'>{otpCode}</span>
</div>
<p style='color: #777; font-size: 13px;'>Mã OTP này có hiệu lực trong vòng <strong>10 phút</strong>. Vì lý do bảo mật, tuyệt đối không chia sẻ mã này cho bất kỳ ai.</p>
<div style='margin-top: 30px; padding-top: 15px; border-top: 1px solid #eee; text-align: center; color: #777; font-size: 12px;'>
<p style='margin: 5px 0;'>PANDORA Jewelry © {DateTime.Now.Year} - Mọi quyền được bảo lưu.</p>
</div>
</div></body></html>";

                SaveLocalEmailCopy(9999, recipientEmail, subject, body);

                if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password) && username != "your_email@gmail.com")
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            using (MailMessage mail = new MailMessage())
                            {
                                mail.From = new MailAddress(username, senderName, Encoding.UTF8);
                                mail.To.Add(new MailAddress(recipientEmail.Trim(), recipientName));
                                mail.Subject = subject;
                                mail.Body = body;
                                mail.IsBodyHtml = true;
                                mail.BodyEncoding = Encoding.UTF8;
                                mail.SubjectEncoding = Encoding.UTF8;

                                using (SmtpClient smtp = new SmtpClient(host, port))
                                {
                                    smtp.Credentials = new NetworkCredential(username, password);
                                    smtp.EnableSsl = enableSsl;
                                    smtp.Send(mail);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("SMTP Otp Exception: " + ex.Message);
                        }
                    });
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SendOtpEmail error: " + ex.Message);
                return false;
            }
        }
    }
}
