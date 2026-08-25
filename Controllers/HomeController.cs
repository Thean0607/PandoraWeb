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
            vm.FlashSaleProducts = db.Products.Include(p => p.Category).Where(p => p.OldPrice != null && p.OldPrice > p.BasePrice).Take(8).ToList();
            vm.LatestBlogs = db.BlogPosts.Where(b => b.IsPublished).OrderByDescending(b => b.PublishedDate).Take(3).ToList();

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
            var faqs = db.Faqs.Where(f => f.IsActive).OrderBy(f => f.DisplayOrder).ToList();
            return View(faqs);
        }

        public ActionResult Stores()
        {
            ViewBag.ActiveMenu = "Stores";
            ViewBag.Title = "Hệ Thống Cửa Hàng";
            return View();
        }

        public ActionResult Blog(int page = 1)
        {
            ViewBag.ActiveMenu = "Blog";
            ViewBag.Title = "Tin Tức";
            
            int pageSize = 6;
            int totalPosts = db.BlogPosts.Count(b => b.IsPublished);
            var posts = db.BlogPosts.Where(b => b.IsPublished)
                                    .OrderByDescending(b => b.PublishedDate)
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)System.Math.Ceiling((double)totalPosts / pageSize);
            
            return View(posts);
        }

        public ActionResult BlogDetail(int id)
        {
            var post = db.BlogPosts.FirstOrDefault(b => b.PostId == id && b.IsPublished);
            if (post == null) return HttpNotFound();

            ViewBag.ActiveMenu = "Blog";
            ViewBag.Title = post.Title;
            return View(post);
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

