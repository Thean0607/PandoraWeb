using PandoraWeb.Filters;
using PandoraWeb.Models.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace PandoraWeb.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class ReportsController : Controller
    {
        private PandoraDbContext db = new PandoraDbContext();

        [AdminAuthorize(Permission = "read_report")]
        public ActionResult SalesReports()
        {
            ViewBag.ActiveMenu = "Reports";
            ViewBag.ActiveSubMenu = "SalesReports";
            ViewBag.Title = "Báo Cáo Doanh Thu";
            var orders = db.Orders.Where(o => o.PaymentStatus == "Paid").ToList();
            ViewBag.TotalRevenue = orders.Sum(o => o.TotalAmount);
            ViewBag.TotalOrders = orders.Count;
            var recentOrders = orders.OrderByDescending(o => o.OrderDate).Take(50).ToList();
            return View(recentOrders);
        }

        [AdminAuthorize(Permission = "read_report")]
        public ActionResult InventoryReports()
        {
            ViewBag.ActiveMenu = "Reports";
            ViewBag.ActiveSubMenu = "InventoryReports";
            ViewBag.Title = "Báo Cáo Tồn Kho";
            var inventory = db.ProductVariants.Include(v => v.Product).Include(v => v.Size).Include(v => v.Material).OrderBy(v => v.Stock).ToList();
            return View(inventory);
        }

        [AdminAuthorize(Permission = "manage_product")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddStock(int variantId, int quantity)
        {
            if (quantity <= 0)
                return Json(new { success = false, message = "Số lượng nhập phải lớn hơn 0" });

            try
            {
                var variant = db.ProductVariants.Find(variantId);
                if (variant == null)
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm" });

                variant.Stock += quantity;
                db.SaveChanges();

                return Json(new { success = true, message = "Nhập kho thành công!" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
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
