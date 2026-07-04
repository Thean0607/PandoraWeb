using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using PandoraWeb.Models.Data;

namespace PandoraWeb.Controllers
{
    public class ProductController : Controller
    {
        private PandoraDbContext db = new PandoraDbContext();

        public ActionResult Category()
        {
            ViewBag.ActiveMenu = "Category";
            ViewBag.Title = "Bộ Sưu Tập";

            // Lấy tất cả sản phẩm kèm danh mục (Eager Loading)
            var products = db.Products.Include(p => p.Category).ToList();

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
