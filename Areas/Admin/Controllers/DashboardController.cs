using System.Web.Mvc;
using PandoraWeb.Filters;
using PandoraWeb.Models;
using PandoraWeb.Models.Data;
using System.Linq;
using System.Data.Entity;

namespace PandoraWeb.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class DashboardController : Controller
    {
        private PandoraDbContext db = new PandoraDbContext();

        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "Dashboard";
            ViewBag.Title = "Tổng Quan";

            ViewBag.TotalRevenue = db.Orders.Where(o => o.PaymentStatus == "Paid").Sum(o => (decimal?)o.TotalAmount) ?? 0m;
            ViewBag.TotalProducts = db.Products.Count();
            ViewBag.TotalCustomers = db.Customers.Count();
            ViewBag.TotalNewOrders = db.Orders.Count(o => o.OrderStatus == "Pending");

            var recentOrders = db.Orders.Include(o => o.Customer).OrderByDescending(o => o.OrderDate).Take(10).ToList();
            
            return View(recentOrders);
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
