using System.Web.Mvc;
using PandoraWeb.Filters;
using PandoraWeb.Models;
using PandoraWeb.Models.Data;
using System.Linq;
using System.Data.Entity;
using System;

namespace PandoraWeb.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class ProductsController : Controller
    {
        private PandoraDbContext db = new PandoraDbContext();

        // GET: Admin/Products
        [AdminAuthorize(Permission = "manage_product")]
        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "Catalog";
            ViewBag.ActiveSubMenu = "Products";
            ViewBag.Title = "Quản lý Sản Phẩm";
            var products = db.Products.Include(p => p.Category).Include(p => p.Collection).Include(p => p.ProductImages).OrderByDescending(p => p.ProductId).ToList();
            ViewBag.Categories = db.Categories.ToList();
            ViewBag.Collections = db.Collections.ToList();
            return View("Products", products); // Point to Products view
        }

        [AdminAuthorize(Permission = "manage_product")]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveProduct(int? productId, string productName, int categoryId, int? collectionId, string price, int stock, string status, string description, System.Web.HttpPostedFileBase imageFile, System.Collections.Generic.IEnumerable<System.Web.HttpPostedFileBase> extraImages)
        {
            if (string.IsNullOrEmpty(productName))
            {
                TempData["Error"] = "Tên sản phẩm không được để trống!";
                return RedirectToAction("Index");
            }

            var productNameLower = productName.Trim().ToLower();
            bool isDuplicate = false;
            if (productId.HasValue && productId.Value > 0)
            {
                isDuplicate = db.Products.Any(p => p.ProductName.Trim().ToLower() == productNameLower && p.ProductId != productId.Value);
            }
            else
            {
                isDuplicate = db.Products.Any(p => p.ProductName.Trim().ToLower() == productNameLower);
            }
            
            if (isDuplicate)
            {
                TempData["Error"] = "Tên sản phẩm đã tồn tại!";
                return RedirectToAction("Index");
            }
            
            decimal parsedPrice = 0;
            if (!string.IsNullOrEmpty(price))
            {
                decimal.TryParse(price.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsedPrice);
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
                    p.CollectionId = collectionId;
                    p.Description = description;
                    p.BasePrice = parsedPrice;
                    p.Status = status;
                    p.UpdatedAt = DateTime.Now;
                    
                    if (Request.Form["removeMainImage"] == "true")
                    {
                        p.ImageUrl = "assets/img/products/default.jpg";
                    }
                    else if (imageUrl != null)
                    {
                        p.ImageUrl = imageUrl;
                    }

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
                    CollectionId = collectionId,
                    Description = description,
                    BasePrice = parsedPrice,
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

            int targetProductId = productId ?? db.Products.Max(prod => prod.ProductId);

            // Handle Extra Images Upload
            if (extraImages != null)
            {
                var cloudinaryHelper = new PandoraWeb.Helpers.CloudinaryHelper();
                foreach (var file in extraImages)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        string extraUrl = cloudinaryHelper.UploadImage(file);
                        if (!string.IsNullOrEmpty(extraUrl))
                        {
                            var pImage = new ProductImage
                            {
                                ProductId = targetProductId,
                                ImageUrl = extraUrl,
                                IsPrimary = false,
                                DisplayOrder = 0
                            };
                            db.ProductImages.Add(pImage);
                        }
                    }
                }
                db.SaveChanges();
            }
            
            PandoraWeb.Helpers.LogHelper.LogActivity("Employee", Session["EmployeeId"] as int?, "SAVE_PRODUCT", $"Đã thêm/sửa sản phẩm ID: {targetProductId}");
            TempData["Success"] = "Đã lưu sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        [AdminAuthorize(Permission = "manage_product")]
        [HttpPost]
        public ActionResult DeleteProduct(int id)
        {
            try
            {
                var p = db.Products.Find(id);
                if (p != null)
                {
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

        [AdminAuthorize(Permission = "manage_product")]
        public ActionResult Collections()
        {
            ViewBag.ActiveMenu = "Catalog";
            ViewBag.ActiveSubMenu = "Collections";
            ViewBag.Title = "Quản lý Bộ Sưu Tập";
            var collections = db.Collections.OrderByDescending(c => c.CollectionId).ToList();
            return View(collections);
        }

        [AdminAuthorize(Permission = "manage_product")]
        [HttpPost]
        public ActionResult SaveCollection(int? collectionId, string collectionName, string description, System.Web.HttpPostedFileBase imageFile)
        {
            if (string.IsNullOrEmpty(collectionName))
            {
                TempData["Error"] = "Tên bộ sưu tập không được để trống!";
                return RedirectToAction("Collections");
            }

            var collectionNameLower = collectionName.Trim().ToLower();
            bool isDuplicate = false;
            if (collectionId.HasValue && collectionId.Value > 0)
            {
                isDuplicate = db.Collections.Any(c => c.CollectionName.Trim().ToLower() == collectionNameLower && c.CollectionId != collectionId.Value);
            }
            else
            {
                isDuplicate = db.Collections.Any(c => c.CollectionName.Trim().ToLower() == collectionNameLower);
            }

            if (isDuplicate)
            {
                TempData["Error"] = "Tên bộ sưu tập đã tồn tại!";
                return RedirectToAction("Collections");
            }

            string imageUrl = null;
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                var cloudinaryHelper = new PandoraWeb.Helpers.CloudinaryHelper();
                imageUrl = cloudinaryHelper.UploadImage(imageFile);
            }

            if (collectionId.HasValue && collectionId.Value > 0)
            {
                var c = db.Collections.Find(collectionId.Value);
                if (c != null)
                {
                    c.CollectionName = collectionName;
                    c.Description = description;
                    if (imageUrl != null) c.ImageUrl = imageUrl;
                }
            }
            else
            {
                var c = new Collection
                {
                    CollectionName = collectionName,
                    Description = description,
                    ImageUrl = imageUrl ?? "assets/img/collections/default.jpg"
                };
                db.Collections.Add(c);
            }
            db.SaveChanges();
            TempData["Success"] = "Đã lưu bộ sưu tập thành công!";
            return RedirectToAction("Collections");
        }

        [AdminAuthorize(Permission = "manage_product")]
        [HttpPost]
        public ActionResult DeleteCollection(int id)
        {
            try
            {
                var c = db.Collections.Find(id);
                if (c != null)
                {
                    var products = db.Products.Where(p => p.CollectionId == id).ToList();
                    foreach (var p in products) p.CollectionId = null;
                    
                    db.Collections.Remove(c);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Đã xóa bộ sưu tập." });
                }
                return Json(new { success = false, message = "Không tìm thấy bộ sưu tập." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AdminAuthorize(Permission = "manage_product")]
        public ActionResult Categories()
        {
            ViewBag.ActiveMenu = "Catalog";
            ViewBag.ActiveSubMenu = "Categories";
            ViewBag.Title = "Danh Mục Sản Phẩm";
            var categories = db.Categories.OrderByDescending(c => c.CategoryId).ToList();
            return View(categories);
        }

        [AdminAuthorize(Permission = "manage_product")]
        [HttpPost]
        public ActionResult SaveCategory(int? id, string name)
        {
            if (string.IsNullOrEmpty(name)) return Json(new { success = false, message = "Tên không được để trống" });

            var nameLower = name.Trim().ToLower();
            bool isDuplicate = false;
            if (id.HasValue && id.Value > 0)
            {
                isDuplicate = db.Categories.Any(c => c.CategoryName.Trim().ToLower() == nameLower && c.CategoryId != id.Value);
            }
            else
            {
                isDuplicate = db.Categories.Any(c => c.CategoryName.Trim().ToLower() == nameLower);
            }

            if (isDuplicate)
            {
                return Json(new { success = false, message = "Tên danh mục đã tồn tại!" });
            }

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

        [AdminAuthorize(Permission = "manage_product")]
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

        [AdminAuthorize(Permission = "manage_product")]
        public ActionResult Brands()
        {
            ViewBag.ActiveMenu = "Catalog";
            ViewBag.ActiveSubMenu = "Brands";
            ViewBag.Title = "Nhãn Hiệu (Collections)";
            var brands = db.Collections.OrderByDescending(c => c.CollectionId).ToList();
            return View(brands);
        }

        [AdminAuthorize(Permission = "manage_product")]
        [HttpPost]
        public ActionResult SaveBrand(int? id, string name, string description, System.Web.HttpPostedFileBase imageFile)
        {
            if (string.IsNullOrEmpty(name)) return RedirectToAction("Brands");

            var nameLower = name.Trim().ToLower();
            bool isDuplicate = false;
            if (id.HasValue && id.Value > 0)
            {
                isDuplicate = db.Collections.Any(c => c.CollectionName.Trim().ToLower() == nameLower && c.CollectionId != id.Value);
            }
            else
            {
                isDuplicate = db.Collections.Any(c => c.CollectionName.Trim().ToLower() == nameLower);
            }

            if (isDuplicate)
            {
                TempData["Error"] = "Tên nhãn hiệu đã tồn tại!";
                return RedirectToAction("Brands");
            }

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

        [AdminAuthorize(Permission = "manage_product")]
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

        [AdminAuthorize(Permission = "manage_product")]
        public ActionResult Attributes()
        {
            ViewBag.ActiveMenu = "Catalog";
            ViewBag.ActiveSubMenu = "Attributes";
            ViewBag.Title = "Thuộc Tính Sản Phẩm";
            ViewBag.Materials = db.Materials.OrderByDescending(m => m.MaterialId).ToList();
            ViewBag.Sizes = db.Sizes.OrderByDescending(s => s.SizeId).ToList();
            return View();
        }

        [AdminAuthorize(Permission = "manage_product")]
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
        
        [AdminAuthorize(Permission = "manage_product")]
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

        [AdminAuthorize(Permission = "manage_product")]
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
        
        [AdminAuthorize(Permission = "manage_product")]
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
