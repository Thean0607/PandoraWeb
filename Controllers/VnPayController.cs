using PandoraWeb.Helpers;
using PandoraWeb.Models;
using PandoraWeb.Models.Data;
using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace PandoraWeb.Controllers
{
    public class VnPayController : Controller
    {
        private PandoraDbContext db = new PandoraDbContext();

        public ActionResult Checkout(int orderId)
        {
            var order = db.Orders.Find(orderId);
            if (order == null)
            {
                return HttpNotFound();
            }

            // Lấy cấu hình từ Web.config
            string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"];
            string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"];
            string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"];
            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];

            // Xây dựng URL động cho ReturnUrl và IpnUrl (hỗ trợ cả localhost ngrok và server thật)
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority;
            string returnUrl = baseUrl + vnp_Returnurl;

            VnPayLibrary vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            // Số tiền thanh toán. VNPAY yêu cầu nhân 100
            vnpay.AddRequestData("vnp_Amount", (order.TotalAmount * 100).ToString("0"));
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", VnPayLibrary.GetIpAddress());
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang: " + order.OrderId);
            vnpay.AddRequestData("vnp_OrderType", "other"); // default
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", order.OrderId.ToString()); // Mã tham chiếu (mã đơn hàng)

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return Redirect(paymentUrl);
        }

        public ActionResult VnPayReturn()
        {
            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }

                int orderId = Convert.ToInt32(vnpay.GetResponseData("vnp_TxnRef"));
                long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_SecureHash = Request.QueryString["vnp_SecureHash"];

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    var order = db.Orders.Find(orderId);
                    if (order != null)
                    {
                        if (vnp_ResponseCode == "00")
                        {
                            // Thanh toán thành công
                            if (order.PaymentStatus != "Paid")
                            {
                                order.PaymentStatus = "Paid";
                                db.SaveChanges();
                            }
                            ViewBag.Message = "Thanh toán thành công hóa đơn " + orderId;
                            // Redirect về trang OrderSuccess thay vì view trống
                            return RedirectToAction("OrderSuccess", "Checkout", new { id = orderId });
                        }
                        else
                        {
                            // Thanh toán lỗi
                            order.PaymentStatus = "Failed";
                            db.SaveChanges();
                            ViewBag.Message = "Có lỗi xảy ra trong quá trình thanh toán hóa đơn " + orderId + " | Mã lỗi: " + vnp_ResponseCode;
                        }
                    }
                    else
                    {
                        ViewBag.Message = "Không tìm thấy đơn hàng " + orderId;
                    }
                }
                else
                {
                    ViewBag.Message = "Lỗi xác thực chữ ký (Invalid signature)";
                }
            }
            return View();
        }

        // Webhook (IPN) để VNPAY gọi về ngầm, giúp chắc chắn update được trạng thái dù khách tắt trình duyệt
        public ActionResult VnPayIpn()
        {
            string returnContent = string.Empty;
            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }

                int orderId = Convert.ToInt32(vnpay.GetResponseData("vnp_TxnRef"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_SecureHash = Request.QueryString["vnp_SecureHash"];

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    var order = db.Orders.Find(orderId);
                    if (order != null)
                    {
                        if (order.TotalAmount * 100 == Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")))
                        {
                            if (order.PaymentStatus != "Paid")
                            {
                                if (vnp_ResponseCode == "00")
                                {
                                    order.PaymentStatus = "Paid";
                                }
                                else
                                {
                                    order.PaymentStatus = "Failed";
                                }
                                db.SaveChanges();
                                returnContent = "{\"RspCode\":\"00\",\"Message\":\"Confirm Success\"}";
                            }
                            else
                            {
                                returnContent = "{\"RspCode\":\"02\",\"Message\":\"Order already confirmed\"}";
                            }
                        }
                        else
                        {
                            returnContent = "{\"RspCode\":\"04\",\"Message\":\"invalid amount\"}";
                        }
                    }
                    else
                    {
                        returnContent = "{\"RspCode\":\"01\",\"Message\":\"Order not found\"}";
                    }
                }
                else
                {
                    returnContent = "{\"RspCode\":\"97\",\"Message\":\"Invalid signature\"}";
                }
            }
            else
            {
                returnContent = "{\"RspCode\":\"99\",\"Message\":\"Input data required\"}";
            }

            return Content(returnContent, "application/json");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
