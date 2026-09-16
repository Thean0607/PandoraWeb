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
    public class ProductsController : Controller
    {
        private PandoraDbContext db = new PandoraDbContext();

        [AdminAuthorize(Permission = "manage_product")]
        public ActionResult Index(int? page)
        {
            ViewBag.ActiveMenu = "Products";
            ViewBag.ActiveSubMenu = "ProductsList";
            ViewBag.Title = "Quản lý Sản phẩm";
            int pageSize = 15;
            int pageNumber = (page ?? 1);
            var products = db.Products.Include(p => p.Category)
                             .Where(p => p.Status != "deleted")
                             .OrderByDescending(p => p.ProductId)
                             .ToPagedList(pageNumber, pageSize);
            return View("Products", products);
        }

        [AdminAuthorize(Permission = "manage_product")]
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult SaveProduct(int? productId, string productName, int categoryId, decimal basePrice, decimal? oldPrice, string status, string description, System.Web.HttpPostedFileBase imageFile)
        {
            var sanitizer = new Ganss.Xss.HtmlSanitizer();
            string safeDescription = !string.IsNullOrEmpty(description) ? sanitizer.Sanitize(description) : "";

            if (productId.HasValue && productId.Value > 0)
            {
                var product = db.Products.Find(productId.Value);
                if (product != null)
                {
                    product.ProductName = productName;
                    product.CategoryId = categoryId;
                    product.BasePrice = basePrice;
                    product.OldPrice = oldPrice;
                    product.Status = status;
                    product.Description = safeDescription;
                    product.UpdatedAt = DateTime.Now;

                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        var cloudinaryHelper = new PandoraWeb.Helpers.CloudinaryHelper();
                        string uploadedUrl = cloudinaryHelper.UploadImage(imageFile);
                        if (!string.IsNullOrEmpty(uploadedUrl))
                        {
                            product.ImageUrl = uploadedUrl;
                        }
                    }
                }
            }
            else
            {
                var newProduct = new Product
                {
                    ProductName = productName,
                    CategoryId = categoryId,
                    BasePrice = basePrice,
                    OldPrice = oldPrice,
                    Status = status,
                    Description = safeDescription,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var cloudinaryHelper = new PandoraWeb.Helpers.CloudinaryHelper();
                    string uploadedUrl = cloudinaryHelper.UploadImage(imageFile);
                    if (!string.IsNullOrEmpty(uploadedUrl))
                    {
                        newProduct.ImageUrl = uploadedUrl;
                    }
                }
                db.Products.Add(newProduct);
            }
            db.SaveChanges();
            TempData["Success"] = "Đã lưu sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        [AdminAuthorize(Permission = "manage_product")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProduct(int id)
        {
            var p = db.Products.Find(id);
            if (p != null)
            {
                // Soft delete
                p.Status = "deleted";
                p.UpdatedAt = DateTime.Now;
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [AdminAuthorize(Permission = "manage_product")]
        public ActionResult Categories()
        {
            ViewBag.ActiveMenu = "Products";
            ViewBag.ActiveSubMenu = "Categories";
            ViewBag.Title = "Danh Mục";
            var cats = db.Categories.ToList();
            return View(cats);
        }

        [AdminAuthorize(Permission = "manage_product")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCategory(int? id, string name, string status)
        {
            if (id.HasValue && id.Value > 0)
            {
                var c = db.Categories.Find(id.Value);
                if (c != null) { c.CategoryName = name;  }
            }
            else { db.Categories.Add(new Category { CategoryName = name }); }
            db.SaveChanges();
            return Json(new { success = true });
        }

        [AdminAuthorize(Permission = "manage_product")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCategory(int id)
        {
            var c = db.Categories.Find(id);
            if (c != null) { db.Categories.Remove(c); db.SaveChanges(); return Json(new { success = true }); }
            return Json(new { success = false });
        }

        [AdminAuthorize(Permission = "manage_product")]
        public ActionResult Brands()
        {
            ViewBag.ActiveMenu = "Products";
            ViewBag.ActiveSubMenu = "Brands";
            ViewBag.Title = "Nhãn Hiệu";
            return View();
        }

        [AdminAuthorize(Permission = "manage_product")]
        public ActionResult Collections()
        {
            ViewBag.ActiveMenu = "Products";
            ViewBag.ActiveSubMenu = "Collections";
            ViewBag.Title = "Bộ Sưu Tập";
            var cols = db.Collections.ToList();
            return View(cols);
        }

        [AdminAuthorize(Permission = "manage_product")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCollection(int? id, string name, string status)
        {
            if (id.HasValue && id.Value > 0)
            {
                var c = db.Collections.Find(id.Value);
                if (c != null) { c.CollectionName = name;  }
            }
            else { db.Collections.Add(new Collection { CollectionName = name }); }
            db.SaveChanges();
            return Json(new { success = true });
        }

        [AdminAuthorize(Permission = "manage_product")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCollection(int id)
        {
            var c = db.Collections.Find(id);
            if (c != null) { db.Collections.Remove(c); db.SaveChanges(); return Json(new { success = true }); }
            return Json(new { success = false });
        }

        [AdminAuthorize(Permission = "manage_product")]
        public ActionResult Attributes()
        {
            ViewBag.ActiveMenu = "Products";
            ViewBag.ActiveSubMenu = "Attributes";
            ViewBag.Title = "Thuộc Tính Sản Phẩm";
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


