using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using PandoraWeb.Models;
using PandoraWeb.Models.Data;

namespace PandoraWeb.Controllers
{
    public class OrderController : Controller
    {
        private PandoraDbContext db = new PandoraDbContext();

        public ActionResult Lookup()
        {
            ViewBag.ActiveMenu = "Lookup";
            ViewBag.Title = "Tra Cứu Đơn Hàng";
            return View();
        }

        [HttpPost]
        public ActionResult Lookup(string orderId, string searchKey)
        {
            ViewBag.ActiveMenu = "Lookup";
            ViewBag.Title = "Tra Cứu Đơn Hàng";
            ViewBag.OrderIdVal = orderId;
            ViewBag.SearchKeyVal = searchKey;

            if (string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(searchKey))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập Mã đơn hàng và Số điện thoại / Email để tra cứu.";
                return View();
            }

            string cleanIdStr = orderId.Replace("#", "").Replace("PAN", "").Replace("pan", "").Trim();
            int id;
            if (!int.TryParse(cleanIdStr, out id))
            {
                ViewBag.ErrorMessage = "Mã đơn hàng không hợp lệ (Ví dụ hợp lệ: #PAN1 hoặc 1).";
                return View();
            }

            string cleanKey = searchKey.Trim().ToLower();

            var order = db.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems.Select(i => i.Variant.Product))
                .FirstOrDefault(o => o.OrderId == id &&
                    (
                        (o.Customer != null && o.Customer.Email.ToLower() == cleanKey) ||
                        (o.Customer != null && o.Customer.PhoneNumber != null && o.Customer.PhoneNumber.Contains(cleanKey)) ||
                        (o.ShippingAddress != null && o.ShippingAddress.PhoneNumber != null && o.ShippingAddress.PhoneNumber.Contains(cleanKey)) ||
                        (o.ShippingAddress != null && o.ShippingAddress.ReceiverName != null && o.ShippingAddress.ReceiverName.ToLower().Contains(cleanKey))
                    ));

            if (order == null)
            {
                ViewBag.ErrorMessage = "Không tìm thấy đơn hàng phù hợp với Mã đơn và SĐT/Email đã nhập.";
                return View();
            }

            return View(order);
        }

        public ActionResult Orders()
        {
            ViewBag.ActiveMenu = "Orders";
            ViewBag.Title = "Lịch Sử Đơn Hàng";

            if (Session["CustomerId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int customerId = (int)Session["CustomerId"];
            ViewBag.Customer = db.Customers.Find(customerId);

            var orders = db.Orders
                .Include(o => o.OrderItems.Select(i => i.Variant.Product))
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
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
