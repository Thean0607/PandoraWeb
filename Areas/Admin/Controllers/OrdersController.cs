using System.Web.Mvc;
using PandoraWeb.Filters;
using PandoraWeb.Models;
using PandoraWeb.Models.Data;
using System.Linq;
using System.Data.Entity;
using System;

namespace PandoraWeb.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class OrdersController : Controller
    {
        private PandoraDbContext db = new PandoraDbContext();

        [AdminAuthorize(Permission = "manage_order")]
        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "Orders";
            ViewBag.ActiveSubMenu = "OrdersList";
            ViewBag.Title = "Danh sách Đơn Hàng";
            var orders = db.Orders.Include(o => o.Customer).OrderByDescending(o => o.OrderDate).ToList();
            return View("Orders", orders);
        }

        [AdminAuthorize(Permission = "manage_order")]
        public ActionResult OrderDetails(int id)
        {
            ViewBag.ActiveMenu = "Orders";
            ViewBag.ActiveSubMenu = "OrdersList";
            ViewBag.Title = $"Chi Tiết Đơn Hàng #PAN{id}";

            var order = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderItems.Select(i => i.Variant.Product))
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return HttpNotFound("Không tìm thấy đơn hàng.");
            }

            return View(order);
        }

        [AdminAuthorize(Permission = "manage_order")]
        [HttpPost]
        public ActionResult UpdateOrderStatus(int id, string status)
        {
            try {
                var order = db.Orders.Find(id);
                if (order != null) {
                    order.OrderStatus = status;
                    db.SaveChanges();
                    PandoraWeb.Helpers.LogHelper.LogActivity("Employee", Session["EmployeeId"] as int?, "UPDATE_ORDER_STATUS", $"Cập nhật trạng thái đơn hàng {id} thành {status}");
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            } catch(Exception e) { return Json(new { success = false, message = e.Message }); }
        }

        [HttpPost]
        [AdminAuthorize(Permission = "manage_order")]
        public ActionResult UpdatePaymentStatus(int id, string status)
        {
            try {
                var order = db.Orders.Find(id);
                if (order != null) {
                    order.PaymentStatus = status;
                    db.SaveChanges();
                    PandoraWeb.Helpers.LogHelper.LogActivity("Employee", Session["EmployeeId"] as int?, "UPDATE_PAYMENT_STATUS", $"Cập nhật trạng thái thanh toán đơn hàng {id} thành {status}");
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            } catch(Exception e) { return Json(new { success = false, message = e.Message }); }
        }

        [AdminAuthorize(Permission = "manage_order")]
        public ActionResult Refunds()
        {
            ViewBag.ActiveMenu = "Orders";
            ViewBag.ActiveSubMenu = "Refunds";
            ViewBag.Title = "Hoàn Trả / Hủy";
            var refunds = db.Orders.Include(o => o.Customer)
                            .Where(o => o.OrderStatus == "Cancelled" || o.OrderStatus == "Refunded")
                            .OrderByDescending(o => o.OrderDate).ToList();
            return View(refunds);
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
