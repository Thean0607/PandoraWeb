using System.Web.Mvc;
using PandoraWeb.Filters;
using PandoraWeb.Models;
using PandoraWeb.Models.Data;
using System.Linq;
using System.Data.Entity;
using System.IO;
using System;

namespace PandoraWeb.Controllers
{
    [AdminAuthorize]
    public class AdminController : Controller
    {
        private PandoraDbContext db = new PandoraDbContext();

        // GET: Admin/Index
        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "Dashboard";
            ViewBag.Title = "Tổng Quan";
            return View();
        }

        // GET: Admin/Products
        public ActionResult Products()
        {
            ViewBag.ActiveMenu = "Catalog";
            ViewBag.ActiveSubMenu = "Products";
            ViewBag.Title = "Quản lý Sản Phẩm";
            var products = db.Products.Include(p => p.Category).OrderByDescending(p => p.ProductId).ToList();
            ViewBag.Categories = db.Categories.ToList();
            return View(products);
        }

        [HttpPost]
        public ActionResult SaveProduct(int? productId, string productName, int categoryId, decimal price, int stock, string status, System.Web.HttpPostedFileBase imageFile)
        {
            if (string.IsNullOrEmpty(productName))
            {
                TempData["Error"] = "Tên sản phẩm không được để trống!";
                return RedirectToAction("Products");
            }

            string imageUrl = null;

            // Handle Image Upload
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                var cloudinaryHelper = new PandoraWeb.Helpers.CloudinaryHelper();
                imageUrl = cloudinaryHelper.UploadImage(imageFile);
            }

            if (productId.HasValue && productId.Value > 0)
            {
                // Update
                var p = db.Products.Find(productId.Value);
                if (p != null)
                {
                    p.ProductName = productName;
                    p.CategoryId = categoryId;
                    p.BasePrice = price;
                    p.Status = status;
                    p.UpdatedAt = DateTime.Now;
                    
                    if (imageUrl != null)
                    {
                        p.ImageUrl = imageUrl;
                    }
                    // Since Stock is handled via variants, for a simple implementation we might just update a default variant or not touch it if it's complex.
                    // But for this project, let's assume we don't have direct Stock on Product table (Wait, let me check Product model)
                    // Product model does NOT have Stock property. Stock is in ProductVariant.
                    // I will find the first variant and update its stock, or create one if none exists.
                    var variant = db.ProductVariants.FirstOrDefault(v => v.ProductId == p.ProductId);
                    if (variant != null)
                    {
                        variant.Stock = stock;
                    }
                    else
                    {
                        db.ProductVariants.Add(new ProductVariant { ProductId = p.ProductId, SKU = "SKU-" + p.ProductId, Stock = stock, PriceAdjustment = 0 });
                    }
                }
            }
            else
            {
                // Insert
                var p = new Product
                {
                    ProductName = productName,
                    CategoryId = categoryId,
                    BasePrice = price,
                    Status = status,
                    ImageUrl = imageUrl ?? "assets/img/products/default.jpg",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                db.Products.Add(p);
                db.SaveChanges(); // Save to generate ProductId

                // Create default variant for stock
                var variant = new ProductVariant
                {
                    ProductId = p.ProductId,
                    SKU = "SKU-" + p.ProductId,
                    Stock = stock,
                    PriceAdjustment = 0
                };
                db.ProductVariants.Add(variant);
            }

            db.SaveChanges();
            TempData["Success"] = "Đã lưu sản phẩm thành công!";
            return RedirectToAction("Products");
        }

        [HttpPost]
        public ActionResult DeleteProduct(int id)
        {
            try
            {
                var p = db.Products.Find(id);
                if (p != null)
                {
                    // Remove related variants first
                    var variants = db.ProductVariants.Where(v => v.ProductId == id).ToList();
                    db.ProductVariants.RemoveRange(variants);
                    
                    db.Products.Remove(p);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Đã xóa sản phẩm." });
                }
                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Admin/Customers
        public ActionResult Customers()
        {
            ViewBag.ActiveMenu = "Customers";
            ViewBag.ActiveSubMenu = "CustomersList";
            ViewBag.Title = "Danh sách Khách Hàng";
            var customers = db.Customers.OrderByDescending(c => c.CreatedAt).ToList();
            return View(customers);
        }

        // GET: Admin/Employees
        public ActionResult Employees()
        {
            ViewBag.ActiveMenu = "Settings";
            ViewBag.ActiveSubMenu = "Employees";
            ViewBag.Title = "Quản lý Nhân Viên";
            return View();
        }

        // GET: Admin/Roles
        public ActionResult Roles()
        {
            ViewBag.ActiveMenu = "Settings";
            ViewBag.ActiveSubMenu = "Roles";
            ViewBag.Title = "Phân Quyền";
            return View();
        }

        // --- NEW CATALOG ACTIONS ---
        public ActionResult Categories()
        {
            ViewBag.ActiveMenu = "Catalog";
            ViewBag.ActiveSubMenu = "Categories";
            ViewBag.Title = "Danh Mục Sản Phẩm";
            var categories = db.Categories.OrderByDescending(c => c.CategoryId).ToList();
            return View(categories);
        }

        [HttpPost]
        public ActionResult SaveCategory(int? id, string name)
        {
            if (string.IsNullOrEmpty(name)) return Json(new { success = false, message = "Tên không được để trống" });
            if (id.HasValue && id.Value > 0)
            {
                var cat = db.Categories.Find(id.Value);
                if (cat != null) { cat.CategoryName = name; }
            }
            else
            {
                db.Categories.Add(new Category { CategoryName = name });
            }
            db.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult DeleteCategory(int id)
        {
            try
            {
                var cat = db.Categories.Find(id);
                if (cat != null)
                {
                    if (db.Products.Any(p => p.CategoryId == id)) return Json(new { success = false, message = "Không thể xóa vì đã có sản phẩm thuộc danh mục này." });
                    db.Categories.Remove(cat);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Không tìm thấy danh mục" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult Brands()
        {
            ViewBag.ActiveMenu = "Catalog";
            ViewBag.ActiveSubMenu = "Brands";
            ViewBag.Title = "Nhãn Hiệu (Collections)";
            var brands = db.Collections.OrderByDescending(c => c.CollectionId).ToList();
            return View(brands);
        }

        [HttpPost]
        public ActionResult SaveBrand(int? id, string name, string description, System.Web.HttpPostedFileBase imageFile)
        {
            if (string.IsNullOrEmpty(name)) return RedirectToAction("Brands");
            string imageUrl = null;
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                var cloudinaryHelper = new PandoraWeb.Helpers.CloudinaryHelper();
                imageUrl = cloudinaryHelper.UploadImage(imageFile);
            }

            if (id.HasValue && id.Value > 0)
            {
                var b = db.Collections.Find(id.Value);
                if (b != null)
                {
                    b.CollectionName = name;
                    b.Description = description;
                    if (imageUrl != null) b.ImageUrl = imageUrl;
                }
            }
            else
            {
                db.Collections.Add(new Collection { CollectionName = name, Description = description, ImageUrl = imageUrl });
            }
            db.SaveChanges();
            return RedirectToAction("Brands");
        }

        [HttpPost]
        public ActionResult DeleteBrand(int id)
        {
            try
            {
                var b = db.Collections.Find(id);
                if (b != null)
                {
                    if (db.Products.Any(p => p.CollectionId == id)) return Json(new { success = false, message = "Không thể xóa vì đã có sản phẩm thuộc nhãn hiệu này." });
                    db.Collections.Remove(b);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Không tìm thấy nhãn hiệu" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult Attributes()
        {
            ViewBag.ActiveMenu = "Catalog";
            ViewBag.ActiveSubMenu = "Attributes";
            ViewBag.Title = "Thuộc Tính Sản Phẩm";
            ViewBag.Materials = db.Materials.OrderByDescending(m => m.MaterialId).ToList();
            ViewBag.Sizes = db.Sizes.OrderByDescending(s => s.SizeId).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult SaveMaterial(int? id, string name)
        {
            if (string.IsNullOrEmpty(name)) return Json(new { success = false });
            if (id.HasValue && id.Value > 0)
            {
                var m = db.Materials.Find(id.Value);
                if (m != null) m.MaterialName = name;
            }
            else db.Materials.Add(new Material { MaterialName = name });
            db.SaveChanges();
            return Json(new { success = true });
        }
        
        [HttpPost]
        public ActionResult DeleteMaterial(int id)
        {
            try {
                var m = db.Materials.Find(id);
                if (m != null) {
                    if (db.ProductVariants.Any(v => v.MaterialId == id)) return Json(new { success = false, message = "Đang được sử dụng." });
                    db.Materials.Remove(m);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false });
            } catch(Exception e) { return Json(new { success = false, message = e.Message }); }
        }

        [HttpPost]
        public ActionResult SaveSize(int? id, string name)
        {
            if (string.IsNullOrEmpty(name)) return Json(new { success = false });
            if (id.HasValue && id.Value > 0)
            {
                var s = db.Sizes.Find(id.Value);
                if (s != null) s.SizeValue = name;
            }
            else db.Sizes.Add(new Size { SizeValue = name });
            db.SaveChanges();
            return Json(new { success = true });
        }
        
        [HttpPost]
        public ActionResult DeleteSize(int id)
        {
            try {
                var s = db.Sizes.Find(id);
                if (s != null) {
                    if (db.ProductVariants.Any(v => v.SizeId == id)) return Json(new { success = false, message = "Đang được sử dụng." });
                    db.Sizes.Remove(s);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false });
            } catch(Exception e) { return Json(new { success = false, message = e.Message }); }
        }

        // --- NEW ORDERS ACTIONS ---
        public ActionResult Orders()
        {
            ViewBag.ActiveMenu = "Orders";
            ViewBag.ActiveSubMenu = "OrdersList";
            ViewBag.Title = "Danh sách Đơn Hàng";
            var orders = db.Orders.Include(o => o.Customer).OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }

        [HttpPost]
        public ActionResult UpdateOrderStatus(int id, string status)
        {
            try {
                var order = db.Orders.Find(id);
                if (order != null) {
                    order.OrderStatus = status;
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            } catch(Exception e) { return Json(new { success = false, message = e.Message }); }
        }

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

        // --- NEW CUSTOMERS ACTIONS ---
        public ActionResult CustomerSegments()
        {
            ViewBag.ActiveMenu = "Customers";
            ViewBag.ActiveSubMenu = "CustomerSegments";
            ViewBag.Title = "Phân Nhóm Khách Hàng";
            // Group by spending
            var segments = db.Customers.Select(c => new {
                Customer = c,
                TotalSpent = db.Orders.Where(o => o.CustomerId == c.CustomerId && o.PaymentStatus == "Paid").Sum(o => (decimal?)o.TotalAmount) ?? 0
            }).OrderByDescending(x => x.TotalSpent).ToList();
            
            ViewBag.Segments = segments;
            return View();
        }
        
        public ActionResult Reviews()
        {
            ViewBag.ActiveMenu = "Customers";
            ViewBag.ActiveSubMenu = "Reviews";
            ViewBag.Title = "Đánh Giá Sản Phẩm";
            var reviews = db.Reviews.Include(r => r.Product).Include(r => r.Customer).OrderByDescending(r => r.ReviewDate).ToList();
            return View(reviews);
        }

        [HttpPost]
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

        [HttpPost]
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

        // --- NEW MARKETING ACTIONS ---
        public ActionResult Coupons()
        {
            ViewBag.ActiveMenu = "Marketing";
            ViewBag.ActiveSubMenu = "Coupons";
            ViewBag.Title = "Mã Giảm Giá";
            var coupons = db.Promotions.OrderByDescending(p => p.StartDate).ToList();
            return View(coupons);
        }

        [HttpPost]
        public ActionResult SaveCoupon(int? id, string code, int? percent, decimal? amount, DateTime start, DateTime end, bool active)
        {
            if (id.HasValue && id.Value > 0)
            {
                var promo = db.Promotions.Find(id.Value);
                if (promo != null) {
                    promo.Code = code; promo.DiscountPercentage = percent; promo.DiscountAmount = amount;
                    promo.StartDate = start; promo.EndDate = end; promo.IsActive = active;
                }
            }
            else {
                db.Promotions.Add(new Promotion { Code = code, DiscountPercentage = percent, DiscountAmount = amount, StartDate = start, EndDate = end, IsActive = active });
            }
            db.SaveChanges();
            return RedirectToAction("Coupons");
        }
        
        [HttpPost]
        public ActionResult DeleteCoupon(int id)
        {
            var p = db.Promotions.Find(id);
            if (p != null) { db.Promotions.Remove(p); db.SaveChanges(); return Json(new { success = true }); }
            return Json(new { success = false });
        }

        public ActionResult FlashSales()
        {
            ViewBag.ActiveMenu = "Marketing";
            ViewBag.ActiveSubMenu = "FlashSales";
            ViewBag.Title = "Flash Sales";
            var sales = db.Promotions.Where(p => p.DiscountPercentage >= 30).ToList();
            return View(sales);
        }

        public ActionResult Banners()
        {
            ViewBag.ActiveMenu = "Marketing";
            ViewBag.ActiveSubMenu = "Banners";
            ViewBag.Title = "Quản lý Banners";
            var banners = db.Banners.OrderBy(b => b.DisplayOrder).ToList();
            return View(banners);
        }

        // --- NEW CMS ACTIONS ---
        public ActionResult Pages()
        {
            ViewBag.ActiveMenu = "CMS";
            ViewBag.ActiveSubMenu = "Pages";
            ViewBag.Title = "Trang Tĩnh";
            var pages = db.Pages.OrderByDescending(p => p.CreatedAt).ToList();
            return View(pages);
        }
        
        public ActionResult Blog()
        {
            ViewBag.ActiveMenu = "CMS";
            ViewBag.ActiveSubMenu = "Blog";
            ViewBag.Title = "Bài Viết (Blog)";
            var posts = db.BlogPosts.OrderByDescending(p => p.PublishedDate).ToList();
            return View(posts);
        }
        
        public ActionResult FAQ()
        {
            ViewBag.ActiveMenu = "CMS";
            ViewBag.ActiveSubMenu = "FAQ";
            ViewBag.Title = "Câu Hỏi Thường Gặp";
            var faqs = db.Faqs.OrderBy(f => f.DisplayOrder).ToList();
            return View(faqs);
        }

        // --- NEW REPORTS ACTIONS ---
        public ActionResult SalesReports()
        {
            ViewBag.ActiveMenu = "Reports";
            ViewBag.ActiveSubMenu = "SalesReports";
            ViewBag.Title = "Báo Cáo Doanh Thu";
            var orders = db.Orders.Where(o => o.PaymentStatus == "Paid").ToList();
            ViewBag.TotalRevenue = orders.Sum(o => o.TotalAmount);
            ViewBag.TotalOrders = orders.Count;
            // Get recent paid orders for the table
            var recentOrders = orders.OrderByDescending(o => o.OrderDate).Take(50).ToList();
            return View(recentOrders);
        }

        public ActionResult InventoryReports()
        {
            ViewBag.ActiveMenu = "Reports";
            ViewBag.ActiveSubMenu = "InventoryReports";
            ViewBag.Title = "Báo Cáo Tồn Kho";
            var inventory = db.ProductVariants.Include(v => v.Product).Include(v => v.Size).Include(v => v.Material).OrderBy(v => v.Stock).ToList();
            return View(inventory);
        }

        // --- NEW SETTINGS ACTIONS ---
        public ActionResult Settings()
        {
            ViewBag.ActiveMenu = "Settings";
            ViewBag.ActiveSubMenu = "General";
            ViewBag.Title = "Cài Đặt Chung";
            return View();
        }
        public ActionResult Payments()
        {
            ViewBag.ActiveMenu = "Settings";
            ViewBag.ActiveSubMenu = "Payments";
            ViewBag.Title = "Thanh Toán";
            return View();
        }
        public ActionResult Shipping()
        {
            ViewBag.ActiveMenu = "Settings";
            ViewBag.ActiveSubMenu = "Shipping";
            ViewBag.Title = "Vận Chuyển";
            return View();
        }
    }
}
