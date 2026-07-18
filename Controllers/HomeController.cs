using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using PandoraWeb.Models.Data;

namespace PandoraWeb.Controllers
{
    public class HomeController : Controller
    {
        private PandoraDbContext db = new PandoraDbContext();

        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "Home";
            ViewBag.Title = "Trang Chủ";

            var vm = new PandoraWeb.ViewModels.HomeViewModel();
            vm.Banners = db.Banners.Where(b => b.IsActive).OrderBy(b => b.DisplayOrder).ToList();
            vm.Collections = db.Collections.Take(3).ToList();
            vm.TrendingProducts = db.Products.Include(p => p.Category).Take(8).ToList();

            return View(vm);
        }

        public ActionResult About()
        {
            ViewBag.ActiveMenu = "About";
            ViewBag.Title = "Về Chúng Tôi";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.ActiveMenu = "Contact";
            ViewBag.Title = "Liên Hệ";
            return View();
        }

        public ActionResult Faq()
        {
            ViewBag.ActiveMenu = "Faq";
            ViewBag.Title = "Câu Hỏi Thường Gặp";
            return View();
        }

        public ActionResult Stores()
        {
            ViewBag.ActiveMenu = "Stores";
            ViewBag.Title = "Hệ Thống Cửa Hàng";
            return View();
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

