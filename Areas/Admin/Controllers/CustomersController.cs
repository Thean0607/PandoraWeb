using System.Web.Mvc;
using PandoraWeb.Filters;
using PandoraWeb.Models;
using PandoraWeb.Models.Data;
using System.Linq;
using System.Data.Entity;
using System;
using PagedList;

namespace PandoraWeb.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class CustomersController : Controller
    {
        private PandoraDbContext db = new PandoraDbContext();

        [AdminAuthorize(Permission = "manage_customer")]
        public ActionResult Index(int? page)
        {
            ViewBag.ActiveMenu = "Customers";
            ViewBag.ActiveSubMenu = "CustomersList";
            ViewBag.Title = "Danh sách Khách Hàng";
            int pageSize = 15;
            int pageNumber = (page ?? 1);
            var customers = db.Customers.OrderByDescending(c => c.CreatedAt).ToPagedList(pageNumber, pageSize);
            return View("Customers", customers);
        }

        [AdminAuthorize(Permission = "manage_customer")]
        public ActionResult CustomerSegments()
        {
            ViewBag.ActiveMenu = "Customers";
            ViewBag.ActiveSubMenu = "CustomerSegments";
            ViewBag.Title = "Phân Nhóm Khách Hàng";
            // Group by spending
            var segments = db.Customers.Select(c => new PandoraWeb.ViewModels.CustomerSegmentVM {
                Customer = c,
                TotalSpent = db.Orders.Where(o => o.CustomerId == c.CustomerId && o.PaymentStatus == "Paid").Sum(o => (decimal?)o.TotalAmount) ?? 0m
            }).OrderByDescending(x => x.TotalSpent).ToList();
            
            ViewBag.Segments = segments;
            return View();
        }
        
        [AdminAuthorize(Permission = "manage_customer")]
        public ActionResult Reviews(int? page)
        {
            ViewBag.ActiveMenu = "Customers";
            ViewBag.ActiveSubMenu = "Reviews";
            ViewBag.Title = "Đánh Giá Sản Phẩm";
            int pageSize = 15;
            int pageNumber = (page ?? 1);
            var reviews = db.Reviews.Include(r => r.Product).Include(r => r.Customer).OrderByDescending(r => r.ReviewDate).ToPagedList(pageNumber, pageSize);
            return View(reviews);
        }

        [AdminAuthorize(Permission = "manage_customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateReviewStatus(int id, string status)
        {
            try {
                var review = db.Reviews.Find(id);
                if (review != null) {
                    review.Status = status;
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Không tìm thấy đánh giá" });
            } catch(Exception e) { return Json(new { success = false, message = e.Message }); }
        }

        [AdminAuthorize(Permission = "manage_customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteReview(int id)
        {
            try {
                var review = db.Reviews.Find(id);
                if (review != null) {
                    db.Reviews.Remove(review);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false });
            } catch(Exception e) { return Json(new { success = false, message = e.Message }); }
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
