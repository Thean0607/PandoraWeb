using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using PandoraWeb.Models.Data;

namespace PandoraWeb.Controllers
{
    public class ProductController : Controller
    {
        private PandoraDbContext db = new PandoraDbContext();

        [HttpGet]
        public JsonResult SearchSuggestions(string query)
        {
            if (string.IsNullOrEmpty(query))
                return Json(new object[] { }, JsonRequestBehavior.AllowGet);

            var products = db.Products.Include(p => p.Category)
                .Where(p => p.ProductName.Contains(query) || p.Description.Contains(query))
                .Take(5)
                .ToList();

            var result = products.Select(p => new
            {
                p.ProductId,
                p.ProductName,
                PriceFormatted = p.BasePrice.ToString("N0") + " ₫",
                ImageUrl = (p.ImageUrl != null && p.ImageUrl.StartsWith("http")) ? p.ImageUrl : Url.Content("~/" + p.ImageUrl),
                CategoryName = p.Category != null ? p.Category.CategoryName : "",
                DetailUrl = Url.Action("ProductDetail", "Product", new { id = p.ProductId })
            });

            return Json(result, JsonRequestBehavior.AllowGet);
        }

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
                var defaultProduct = db.Products
                    .Include(p => p.Category)
                    .Include(p => p.ProductVariants.Select(v => v.Size))
                    .Include(p => p.ProductVariants.Select(v => v.Material))
                    .Include(p => p.ProductImages)
                    .FirstOrDefault();
                if (defaultProduct == null) return HttpNotFound();
                
                ViewBag.Reviews = db.Reviews.Include(r => r.Customer).Where(r => r.ProductId == defaultProduct.ProductId && r.Status == "Approved").OrderByDescending(r => r.ReviewDate).ToList();
                return View(defaultProduct);
            }

            var product = db.Products
                .Include(p => p.Category)
                .Include(p => p.ProductVariants.Select(v => v.Size))
                .Include(p => p.ProductVariants.Select(v => v.Material))
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.ProductId == id);
            
            if (product == null)
            {
                return HttpNotFound();
            }

            ViewBag.Reviews = db.Reviews.Include(r => r.Customer).Where(r => r.ProductId == id && r.Status == "Approved").OrderByDescending(r => r.ReviewDate).ToList();

            return View(product);
        }

        [HttpPost]
        public ActionResult SubmitReview(int productId, int rating, string comment)
        {
            if (Session["CustomerId"] == null)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để đánh giá." });
            }

            if (rating < 1 || rating > 5)
            {
                return Json(new { success = false, message = "Đánh giá sao không hợp lệ." });
            }

            int customerId = (int)Session["CustomerId"];

            // Check if user already reviewed
            bool alreadyReviewed = db.Reviews.Any(r => r.ProductId == productId && r.CustomerId == customerId);
            if (alreadyReviewed)
            {
                return Json(new { success = false, message = "Bạn đã đánh giá sản phẩm này rồi." });
            }

            var review = new Models.Review
            {
                ProductId = productId,
                CustomerId = customerId,
                Rating = rating,
                Comment = comment,
                ReviewDate = System.DateTime.Now,
                Status = "Approved" // Or "Pending" if you want manual approval
            };

            db.Reviews.Add(review);
            db.SaveChanges();

            return Json(new { success = true, message = "Đánh giá của bạn đã được gửi thành công!" });
        }

        public ActionResult CollectionDetail(int id, string sort)
        {
            var collection = db.Collections
                               .Include(c => c.Products)
                               .Include("Products.Category")
                               .FirstOrDefault(c => c.CollectionId == id);
            
            if (collection == null) return HttpNotFound();

            ViewBag.ActiveMenu = "Collection";
            ViewBag.Title = collection.CollectionName;

            var productsQuery = collection.Products.AsQueryable();

            switch (sort)
            {
                case "price_asc":
                    productsQuery = productsQuery.OrderBy(p => p.BasePrice);
                    break;
                case "price_desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.BasePrice);
                    break;
                case "newest":
                default:
                    productsQuery = productsQuery.OrderByDescending(p => p.CreatedAt);
                    break;
            }
            ViewBag.Sort = sort;

            collection.Products = productsQuery.ToList();

            return View(collection);
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
