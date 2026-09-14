using System.Web.Mvc;
using PandoraWeb.Filters;
using PandoraWeb.Models;
using PandoraWeb.Models.Data;
using System.Linq;
using System.Data.Entity;

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
