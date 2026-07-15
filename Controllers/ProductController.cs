using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using PandoraWeb.Models.Data;

namespace PandoraWeb.Controllers
{
    public class ProductController : Controller
    {
        private PandoraDbContext db = new PandoraDbContext();

        public ActionResult Category(string search, System.Collections.Generic.List<int> cat, decimal? maxPrice, string sort)
        {
            ViewBag.ActiveMenu = "Category";
            ViewBag.Title = "Bộ Sưu Tập";

            // Lấy danh sách danh mục cho sidebar
            ViewBag.Categories = db.Categories.ToList();

            var query = db.Products.Include(p => p.Category).AsQueryable();

            // Lọc theo tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.ProductName.Contains(search) || p.Description.Contains(search));
                ViewBag.Search = search;
            }

            // Lọc theo danh mục
            if (cat != null && cat.Any())
            {
                query = query.Where(p => cat.Contains(p.CategoryId));
                ViewBag.SelectedCategories = cat;
            }
            else
            {
                ViewBag.SelectedCategories = new System.Collections.Generic.List<int>();
            }

            // Lọc theo giá
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.BasePrice <= maxPrice.Value);
                ViewBag.MaxPrice = maxPrice.Value;
            }
            else
            {
                ViewBag.MaxPrice = 50000000;
            }

            // Sắp xếp
            switch (sort)
            {
                case "price_asc":
                    query = query.OrderBy(p => p.BasePrice);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.BasePrice);
                    break;
                case "newest":
                default:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }
            ViewBag.Sort = sort;

            var products = query.ToList();

            return View(products);
        }

        public ActionResult ProductDetail(int? id)
        {
            ViewBag.ActiveMenu = "ProductDetail";
            ViewBag.Title = "Chi Tiết Sản Phẩm";

            if (id == null)
            {
                // Nếu không truyền ID, mặc định lấy sản phẩm đầu tiên hoặc báo lỗi
                var defaultProduct = db.Products.Include(p => p.Category).FirstOrDefault();
                if (defaultProduct == null) return HttpNotFound();
                return View(defaultProduct);
            }

            var product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductId == id);
            
            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
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
