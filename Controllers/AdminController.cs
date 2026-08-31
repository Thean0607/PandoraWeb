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

            ViewBag.TotalRevenue = db.Orders.Where(o => o.PaymentStatus == "Paid").Sum(o => (decimal?)o.TotalAmount) ?? 0m;
            ViewBag.TotalProducts = db.Products.Count();
            ViewBag.TotalCustomers = db.Customers.Count();
            ViewBag.TotalNewOrders = db.Orders.Count(o => o.OrderStatus == "Pending");

            var recentOrders = db.Orders.Include(o => o.Customer).OrderByDescending(o => o.OrderDate).Take(10).ToList();
            
            return View(recentOrders);
        }

        // GET: Admin/Products
        [AdminAuthorize(Permission = "manage_product")]
        public ActionResult Products()
        {
            ViewBag.ActiveMenu = "Catalog";
            ViewBag.ActiveSubMenu = "Products";
            ViewBag.Title = "Quản lý Sản Phẩm";
            var products = db.Products.Include(p => p.Category).Include(p => p.Collection).OrderByDescending(p => p.ProductId).ToList();
            ViewBag.Categories = db.Categories.ToList();
            ViewBag.Collections = db.Collections.ToList();
            return View(products);
        }

        [AdminAuthorize(Permission = "manage_product")]
        [HttpPost]
        public ActionResult SaveProduct(int? productId, string productName, int categoryId, int? collectionId, string price, int stock, string status, string description, System.Web.HttpPostedFileBase imageFile, System.Collections.Generic.IEnumerable<System.Web.HttpPostedFileBase> extraImages)
        {
            if (string.IsNullOrEmpty(productName))
            {
                TempData["Error"] = "Tên sản phẩm không được để trống!";
                return RedirectToAction("Products");
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
                return RedirectToAction("Products");
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

            // Lấy ID sản phẩm cuối cùng sau khi insert (nếu là thêm mới)
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
            return RedirectToAction("Products");
        }

        // GET: Admin/Collections
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
        [AdminAuthorize(Permission = "manage_customer")]
        public ActionResult Customers()
        {
            ViewBag.ActiveMenu = "Customers";
            ViewBag.ActiveSubMenu = "CustomersList";
            ViewBag.Title = "Danh sách Khách Hàng";
            var customers = db.Customers.OrderByDescending(c => c.CreatedAt).ToList();
            return View(customers);
        }

        // GET: Admin/Employees
        [AdminAuthorize(Permission = "manage_employee")]
        public ActionResult Employees()
        {
            ViewBag.ActiveMenu = "Settings";
            ViewBag.ActiveSubMenu = "Employees";
            ViewBag.Title = "Quản lý Nhân Viên";
            var employees = db.Employees.Include(e => e.Role).OrderByDescending(e => e.EmployeeId).ToList();
            ViewBag.Roles = db.Roles.ToList();
            return View(employees);
        }

        [AdminAuthorize(Permission = "manage_employee")]
        [HttpPost]
        public ActionResult SaveEmployee(int? id, string fullName, string email, int roleId, string status, string password)
        {
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email)) 
                return Json(new { success = false, message = "Thiếu thông tin bắt buộc" });
            
            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var emp = db.Employees.Find(id.Value);
                    if (emp != null)
                    {
                        emp.FullName = fullName;
                        emp.Email = email;
                        emp.RoleId = roleId;
                        emp.Status = status;
                        if (!string.IsNullOrEmpty(password))
                        {
                            emp.PasswordHash = PandoraWeb.Helpers.SecurityHelper.HashSHA256(password);
                        }
                    }
                }
                else
                {
                    string finalPass = !string.IsNullOrEmpty(password) ? password : "123456";
                    db.Employees.Add(new Employee
                    {
                        FullName = fullName,
                        Email = email,
                        PasswordHash = PandoraWeb.Helpers.SecurityHelper.HashSHA256(finalPass),
                        RoleId = roleId,
                        Status = status
                    });
                }
                db.SaveChanges();
                return Json(new { success = true, message = "Lưu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AdminAuthorize(Permission = "manage_employee")]
        [HttpPost]
        public ActionResult DeleteEmployee(int id)
        {
            try
            {
                var emp = db.Employees.Find(id);
                if (emp != null)
                {
                    db.Employees.Remove(emp);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Không tìm thấy" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Admin/Roles
        [AdminAuthorize(Permission = "manage_employee")]
        public ActionResult Roles()
        {
            ViewBag.ActiveMenu = "Settings";
            ViewBag.ActiveSubMenu = "Roles";
            ViewBag.Title = "Phân Quyền";
            var roles = db.Roles.OrderBy(r => r.RoleId).ToList();
            return View(roles);
        }

        [AdminAuthorize(Permission = "manage_employee")]
        [HttpPost]
        public ActionResult SaveRole(int? id, string name, string description, string permissions)
        {
            if (string.IsNullOrEmpty(name)) 
                return Json(new { success = false, message = "Tên không được để trống" });

            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var role = db.Roles.Find(id.Value);
                    if (role != null)
                    {
                        role.RoleName = name;
                        role.Permissions = permissions;
                    }
                }
                else
                {
                    db.Roles.Add(new Role
                    {
                        RoleName = name,
                        Permissions = permissions
                    });
                }
                db.SaveChanges();
                return Json(new { success = true, message = "Lưu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AdminAuthorize(Permission = "manage_employee")]
        [HttpPost]
        public ActionResult DeleteRole(int id)
        {
            try
            {
                var role = db.Roles.Find(id);
                if (role != null)
                {
                    if (db.Employees.Any(e => e.RoleId == id))
                        return Json(new { success = false, message = "Không thể xóa Role đang có nhân viên" });

                    db.Roles.Remove(role);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Không tìm thấy" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // --- NEW CATALOG ACTIONS ---
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

        // --- NEW ORDERS ACTIONS ---
        [AdminAuthorize(Permission = "manage_order")]
        public ActionResult Orders()
        {
            ViewBag.ActiveMenu = "Orders";
            ViewBag.ActiveSubMenu = "OrdersList";
            ViewBag.Title = "Danh sách Đơn Hàng";
            var orders = db.Orders.Include(o => o.Customer).OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }

        [AdminAuthorize(Permission = "manage_order")]
        public ActionResult OrderDetails(int id)
        {
            ViewBag.ActiveMenu = "Orders";
            ViewBag.ActiveSubMenu = "OrdersList";
            ViewBag.Title = $"Chi Tiết Đơn Hàng #PAN{id}";

            var order = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderItems.Select(i => i.Variant.Product))
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return HttpNotFound("Không tìm thấy đơn hàng.");
            }

            return View(order);
        }

        [AdminAuthorize(Permission = "manage_order")]
        [HttpPost]
        public ActionResult UpdateOrderStatus(int id, string status)
        {
            try {
                var order = db.Orders.Find(id);
                if (order != null) {
                    order.OrderStatus = status;
                    db.SaveChanges();
                    PandoraWeb.Helpers.LogHelper.LogActivity("Employee", Session["EmployeeId"] as int?, "UPDATE_ORDER_STATUS", $"Cập nhật trạng thái đơn hàng {id} thành {status}");
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            } catch(Exception e) { return Json(new { success = false, message = e.Message }); }
        }

        [AdminAuthorize(Permission = "manage_order")]
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
        [AdminAuthorize(Permission = "manage_customer")]
        public ActionResult CustomerSegments()
        {
            ViewBag.ActiveMenu = "Customers";
            ViewBag.ActiveSubMenu = "CustomerSegments";
            ViewBag.Title = "Phân Nhóm Khách Hàng";
            // Group by spending
            var segments = db.Customers.Select(c => new {
                Customer = c,
                TotalSpent = db.Orders.Where(o => o.CustomerId == c.CustomerId && o.PaymentStatus == "Paid").Sum(o => (decimal?)o.TotalAmount) ?? 0m
            }).OrderByDescending(x => x.TotalSpent).ToList();
            
            ViewBag.Segments = segments;
            return View();
        }
        
        [AdminAuthorize(Permission = "manage_customer")]
        public ActionResult Reviews()
        {
            ViewBag.ActiveMenu = "Customers";
            ViewBag.ActiveSubMenu = "Reviews";
            ViewBag.Title = "Đánh Giá Sản Phẩm";
            var reviews = db.Reviews.Include(r => r.Product).Include(r => r.Customer).OrderByDescending(r => r.ReviewDate).ToList();
            return View(reviews);
        }

        [AdminAuthorize(Permission = "manage_customer")]
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

        [AdminAuthorize(Permission = "manage_customer")]
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
        [AdminAuthorize(Permission = "manage_marketing")]
        public ActionResult Coupons()
        {
            ViewBag.ActiveMenu = "Marketing";
            ViewBag.ActiveSubMenu = "Coupons";
            ViewBag.Title = "Mã Giảm Giá";
            var coupons = db.Promotions.OrderByDescending(p => p.StartDate).ToList();
            return View(coupons);
        }

        [AdminAuthorize(Permission = "manage_marketing")]
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
        
        [AdminAuthorize(Permission = "manage_marketing")]
        [HttpPost]
        public ActionResult DeleteCoupon(int id)
        {
            var p = db.Promotions.Find(id);
            if (p != null) { db.Promotions.Remove(p); db.SaveChanges(); return Json(new { success = true }); }
            return Json(new { success = false });
        }

        [AdminAuthorize(Permission = "manage_marketing")]
        public ActionResult FlashSales()
        {
            ViewBag.ActiveMenu = "Marketing";
            ViewBag.ActiveSubMenu = "FlashSales";
            ViewBag.Title = "Flash Sales";
            var sales = db.Promotions.Where(p => p.DiscountPercentage >= 30).ToList();
            return View(sales);
        }



        // --- NEW CMS ACTIONS ---
        [AdminAuthorize(Permission = "manage_cms")]
        public ActionResult Pages()
        {
            ViewBag.ActiveMenu = "CMS";
            ViewBag.ActiveSubMenu = "Pages";
            ViewBag.Title = "Trang Tĩnh";
            var pages = db.Pages.OrderByDescending(p => p.CreatedAt).ToList();
            return View(pages);
        }
        
        [AdminAuthorize(Permission = "manage_cms")]
        public ActionResult Blog()
        {
            ViewBag.ActiveMenu = "CMS";
            ViewBag.ActiveSubMenu = "Blog";
            ViewBag.Title = "Bài Viết (Blog)";
            var posts = db.BlogPosts.OrderByDescending(p => p.PublishedDate).ToList();
            return View(posts);
        }
        
        [AdminAuthorize(Permission = "manage_cms")]
        public ActionResult FAQ()
        {
            ViewBag.ActiveMenu = "CMS";
            ViewBag.ActiveSubMenu = "FAQ";
            ViewBag.Title = "Câu Hỏi Thường Gặp";
            var faqs = db.Faqs.OrderBy(f => f.DisplayOrder).ToList();
            return View(faqs);
        }

        [AdminAuthorize(Permission = "manage_cms")]
        [HttpPost]
        public ActionResult SaveFaq(int? id, string question, string answer, int displayOrder, bool isActive)
        {
            if (string.IsNullOrEmpty(question) || string.IsNullOrEmpty(answer))
                return Json(new { success = false, message = "Thiếu thông tin" });

            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var faq = db.Faqs.Find(id.Value);
                    if (faq != null)
                    {
                        faq.Question = question;
                        faq.Answer = answer;
                        faq.DisplayOrder = displayOrder;
                        faq.IsActive = isActive;
                    }
                }
                else
                {
                    db.Faqs.Add(new Faq
                    {
                        Question = question,
                        Answer = answer,
                        DisplayOrder = displayOrder,
                        IsActive = isActive
                    });
                }
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AdminAuthorize(Permission = "manage_cms")]
        [HttpPost]
        public ActionResult DeleteFaq(int id)
        {
            try
            {
                var faq = db.Faqs.Find(id);
                if (faq != null)
                {
                    db.Faqs.Remove(faq);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Không tìm thấy" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // --- NEW REPORTS ACTIONS ---
        [AdminAuthorize(Permission = "read_report")]
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

        [AdminAuthorize(Permission = "read_report")]
        public ActionResult InventoryReports()
        {
            ViewBag.ActiveMenu = "Reports";
            ViewBag.ActiveSubMenu = "InventoryReports";
            ViewBag.Title = "Báo Cáo Tồn Kho";
            var inventory = db.ProductVariants.Include(v => v.Product).Include(v => v.Size).Include(v => v.Material).OrderBy(v => v.Stock).ToList();
            return View(inventory);
        }

        // --- NEW SETTINGS ACTIONS ---
        [AdminAuthorize(Permission = "manage_setting")]
        public ActionResult Settings()
        {
            ViewBag.ActiveMenu = "Settings";
            ViewBag.ActiveSubMenu = "General";
            ViewBag.Title = "Cài Đặt Chung";
            return View();
        }

        [AdminAuthorize(Permission = "manage_setting")]
        public ActionResult Payments()
        {
            ViewBag.ActiveMenu = "Settings";
            ViewBag.ActiveSubMenu = "Payments";
            ViewBag.Title = "Thanh Toán";
            return View();
        }

        [AdminAuthorize(Permission = "manage_setting")]
        public ActionResult Shipping()
        {
            ViewBag.ActiveMenu = "Settings";
            ViewBag.ActiveSubMenu = "Shipping";
            ViewBag.Title = "Vận Chuyển";
            return View();
        }

        // --- PROMO POPUP MANAGEMENT ---
        [AdminAuthorize(Permission = "manage_marketing")]
        public ActionResult PromoPopup()
        {
            ViewBag.ActiveMenu = "Marketing";
            ViewBag.ActiveSubMenu = "PromoPopup";
            ViewBag.Title = "Quản Lý Popup Thông Báo Ưu Đãi";
            var settings = PandoraWeb.Helpers.PromoPopupHelper.GetSettings();
            return View(settings);
        }

        [AdminAuthorize(Permission = "manage_marketing")]
        [HttpPost]
        public ActionResult SavePromoPopup(bool isEnabled = false, string title = null, string subtitle = null, string content = null, string couponCode = null, string imageUrl = null, string buttonText = null, string buttonLink = null, string backgroundColor = null, string textColor = null, string popupLayout = null, System.Web.HttpPostedFileBase imageFile = null)
        {
            var settings = PandoraWeb.Helpers.PromoPopupHelper.GetSettings();
            settings.IsEnabled = isEnabled;
            settings.Title = !string.IsNullOrWhiteSpace(title) ? title.Trim() : settings.Title;
            settings.Subtitle = !string.IsNullOrWhiteSpace(subtitle) ? subtitle.Trim() : "";
            settings.Content = !string.IsNullOrWhiteSpace(content) ? content.Trim() : "";
            settings.CouponCode = !string.IsNullOrWhiteSpace(couponCode) ? couponCode.Trim() : "";
            settings.ButtonText = !string.IsNullOrWhiteSpace(buttonText) ? buttonText.Trim() : "KHÁM PHÁ NGAY";
            settings.ButtonLink = !string.IsNullOrWhiteSpace(buttonLink) ? buttonLink.Trim() : "/Product/Category";
            settings.BackgroundColor = !string.IsNullOrWhiteSpace(backgroundColor) ? backgroundColor.Trim() : "#121212";
            settings.TextColor = !string.IsNullOrWhiteSpace(textColor) ? textColor.Trim() : "#FFFFFF";
            settings.PopupLayout = !string.IsNullOrWhiteSpace(popupLayout) ? popupLayout.Trim() : "horizontal";

            if (imageFile != null && imageFile.ContentLength > 0)
            {
                try
                {
                    var cloudinaryHelper = new PandoraWeb.Helpers.CloudinaryHelper();
                    string uploadedUrl = cloudinaryHelper.UploadImage(imageFile);
                    if (!string.IsNullOrEmpty(uploadedUrl))
                    {
                        settings.ImageUrl = uploadedUrl;
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi khi tải ảnh lên Cloudinary: " + ex.Message;
                }
            }
            else if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                settings.ImageUrl = imageUrl.Trim();
            }

            PandoraWeb.Helpers.PromoPopupHelper.SaveSettings(settings);
            TempData["Success"] = "Cập nhật Popup Thông Báo Ưu Đãi thành công!";
            return RedirectToAction("PromoPopup");
        }

        // --- BANNERS MANAGEMENT ---
        [AdminAuthorize(Permission = "manage_marketing")]
        public ActionResult Banners()
        {
            ViewBag.ActiveMenu = "Marketing";
            ViewBag.ActiveSubMenu = "Banners";
            ViewBag.Title = "Quản Lý Banners";
            var banners = db.Banners.OrderBy(b => b.DisplayOrder).ToList();
            return View(banners);
        }

        [AdminAuthorize(Permission = "manage_marketing")]
        [HttpPost]
        public ActionResult SaveBanner(int? bannerId, string title, string linkUrl, int displayOrder = 0, bool isActive = true, System.Web.HttpPostedFileBase imageFile = null)
        {
            Banner banner = null;
            if (bannerId.HasValue && bannerId.Value > 0)
            {
                banner = db.Banners.Find(bannerId.Value);
            }

            if (banner == null)
            {
                banner = new Banner
                {
                    CreatedAt = DateTime.Now
                };
                db.Banners.Add(banner);
            }

            banner.Title = string.IsNullOrWhiteSpace(title) ? "Banner" : title.Trim();
            banner.LinkUrl = string.IsNullOrWhiteSpace(linkUrl) ? "/Product/Category" : linkUrl.Trim();
            banner.DisplayOrder = displayOrder;
            banner.IsActive = isActive;

            if (imageFile != null && imageFile.ContentLength > 0)
            {
                try
                {
                    var cloudinaryHelper = new PandoraWeb.Helpers.CloudinaryHelper();
                    string uploadedUrl = cloudinaryHelper.UploadImage(imageFile);
                    if (!string.IsNullOrEmpty(uploadedUrl))
                    {
                        banner.ImageUrl = uploadedUrl;
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi khi tải ảnh banner lên Cloudinary: " + ex.Message;
                }
            }

            db.SaveChanges();
            TempData["Success"] = "Đã lưu thông tin Banner thành công!";
            return RedirectToAction("Banners");
        }

        [AdminAuthorize(Permission = "manage_marketing")]
        [HttpPost]
        public JsonResult DeleteBanner(int id)
        {
            var banner = db.Banners.Find(id);
            if (banner != null)
            {
                db.Banners.Remove(banner);
                db.SaveChanges();
                return Json(new { success = true, message = "Xóa banner thành công!" });
            }
            return Json(new { success = false, message = "Không tìm thấy banner." });
        }


        [AdminAuthorize(Permission = "manage_cms")]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveBlog(int? postId, string title, string author, bool isPublished, string content, System.Web.HttpPostedFileBase imageFile)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            {
                TempData["Error"] = "Tiêu đề và Nội dung không được để trống!";
                return RedirectToAction("Blog");
            }

            BlogPost post;
            if (postId.HasValue && postId.Value > 0)
            {
                post = db.BlogPosts.Find(postId.Value);
                if (post == null)
                {
                    TempData["Error"] = "Không tìm thấy bài viết!";
                    return RedirectToAction("Blog");
                }
            }
            else
            {
                post = new BlogPost();
                post.PublishedDate = DateTime.Now;
                db.BlogPosts.Add(post);
            }

            post.Title = title.Trim();
            post.Author = string.IsNullOrWhiteSpace(author) ? "Admin" : author.Trim();
            post.IsPublished = isPublished;
            post.Content = content;

            if (imageFile != null && imageFile.ContentLength > 0)
            {
                try
                {
                    var cloudinaryHelper = new PandoraWeb.Helpers.CloudinaryHelper();
                    string uploadedUrl = cloudinaryHelper.UploadImage(imageFile);
                    if (!string.IsNullOrEmpty(uploadedUrl))
                    {
                        post.ImageUrl = uploadedUrl;
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi khi tải ảnh lên Cloudinary: " + ex.Message;
                    return RedirectToAction("Blog");
                }
            }

            db.SaveChanges();
            TempData["Success"] = "Đã lưu bài viết thành công!";
            return RedirectToAction("Blog");
        }

        [AdminAuthorize(Permission = "manage_cms")]
        [HttpPost]
        public JsonResult DeleteBlog(int id)
        {
            var post = db.BlogPosts.Find(id);
            if (post != null)
            {
                db.BlogPosts.Remove(post);
                db.SaveChanges();
                return Json(new { success = true, message = "Xóa bài viết thành công!" });
            }
            return Json(new { success = false, message = "Không tìm thấy bài viết." });
        }
    }
}
