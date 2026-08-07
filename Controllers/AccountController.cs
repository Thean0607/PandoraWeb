using System;
using System.Linq;
using System.Web.Mvc;
using PandoraWeb.Models;
using PandoraWeb.Models.Data;
using PandoraWeb.ViewModels;
using System.Collections.Generic;
using System.Data.Entity;

namespace PandoraWeb.Controllers
{
    public class AccountController : Controller
    {
        private PandoraDbContext db = new PandoraDbContext();

        public AccountController()
        {
            EnsureAvatarColumnExists();
        }

        public ActionResult Login()
        {
            ViewBag.ActiveMenu = "Login";
            ViewBag.Title = "Đăng Nhập";
            return View();
        }

        [HttpPost]
        public ActionResult Login(string loginId, string password)
        {
            // Kiểm tra trong bảng Employees trước (Admin/Manager)
            var emp = db.Employees.Include("Role").FirstOrDefault(e => e.Email == loginId && e.PasswordHash == password);
            if (emp != null)
            {
                Session["EmployeeId"] = emp.EmployeeId;
                Session["FullName"] = emp.FullName;
                Session["Role"] = emp.Role.RoleName;
                return RedirectToAction("Index", "Admin");
            }

            // Kiểm tra trong bảng Customers (Khách hàng)
            var cus = db.Customers.FirstOrDefault(c => (c.Email == loginId || c.PhoneNumber == loginId) && c.PasswordHash == password);
            if (cus != null)
            {
                Session["CustomerId"] = cus.CustomerId;
                Session["FullName"] = cus.FullName;
                Session["Role"] = "Customer";
                
                SyncDbCartToSession(cus.CustomerId);
                
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Email hoặc mật khẩu không đúng!";
            return View();
        }

        private void EnsureAvatarColumnExists()
        {
            try
            {
                db.Database.ExecuteSqlCommand("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Customers]') AND name = 'AvatarUrl') ALTER TABLE [dbo].[Customers] ADD [AvatarUrl] NVARCHAR(500) NULL;");
            }
            catch
            {
            }
        }

        public new ActionResult Profile()
        {
            if (Session["CustomerId"] == null) return RedirectToAction("Login");
            EnsureAvatarColumnExists();
            ViewBag.ActiveMenu = "Profile";
            ViewBag.Title = "Hồ Sơ Của Tôi";

            int customerId = (int)Session["CustomerId"];
            var customer = db.Customers.Find(customerId);
            if (customer == null) return RedirectToAction("Login");

            if (!string.IsNullOrEmpty(customer.AvatarUrl))
            {
                Session["AvatarUrl"] = PandoraWeb.Helpers.ImageHelper.GetImageUrl(customer.AvatarUrl, "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?q=80&w=200&auto=format&fit=crop");
            }

            return View(customer);
        }

        [HttpPost]
        public new ActionResult Profile(string fullName, string email, string phoneNumber, string gender, DateTime? dateOfBirth, System.Web.HttpPostedFileBase avatarFile)
        {
            if (Session["CustomerId"] == null) return RedirectToAction("Login");
            EnsureAvatarColumnExists();
            ViewBag.ActiveMenu = "Profile";
            ViewBag.Title = "Hồ Sơ Của Tôi";

            int customerId = (int)Session["CustomerId"];
            var customer = db.Customers.Find(customerId);
            if (customer == null) return RedirectToAction("Login");

            if (avatarFile != null && avatarFile.ContentLength > 0)
            {
                try
                {
                    var cloudinaryHelper = new PandoraWeb.Helpers.CloudinaryHelper();
                    string uploadedUrl = cloudinaryHelper.UploadImage(avatarFile);
                    if (!string.IsNullOrEmpty(uploadedUrl))
                    {
                        customer.AvatarUrl = uploadedUrl;
                        Session["AvatarUrl"] = PandoraWeb.Helpers.ImageHelper.GetImageUrl(uploadedUrl, "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?q=80&w=200&auto=format&fit=crop");
                    }
                }
                catch
                {
                }
            }

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                customer.FullName = fullName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(email) && email.Trim().ToLower() != customer.Email.ToLower())
            {
                string cleanEmail = email.Trim().ToLower();
                var dupCus = db.Customers.FirstOrDefault(c => c.CustomerId != customerId && c.Email.ToLower() == cleanEmail);
                var dupEmp = db.Employees.FirstOrDefault(e => e.Email.ToLower() == cleanEmail);
                if (dupCus != null || dupEmp != null)
                {
                    ViewBag.Error = "Email này đã được sử dụng bởi một tài khoản khác.";
                    return View(customer);
                }
                customer.Email = email.Trim();
                Session["CustomerEmail"] = customer.Email;
            }

            customer.PhoneNumber = phoneNumber?.Trim();
            customer.Gender = gender;
            customer.DateOfBirth = dateOfBirth;

            db.SaveChanges();

            Session["FullName"] = customer.FullName;
            TempData["SuccessMessage"] = "Cập nhật thông tin tài khoản thành công!";

            return View(customer);
        }

        [HttpPost]
        public ActionResult UploadAvatar(System.Web.HttpPostedFileBase avatarFile)
        {
            EnsureAvatarColumnExists();

            if (Session["CustomerId"] == null)
            {
                return Json(new { success = false, message = "Bạn chưa đăng nhập." });
            }

            if (avatarFile == null || avatarFile.ContentLength == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn file hình ảnh hợp lệ." });
            }

            try
            {
                var cloudinaryHelper = new PandoraWeb.Helpers.CloudinaryHelper();
                string uploadedUrl = cloudinaryHelper.UploadImage(avatarFile);

                if (!string.IsNullOrEmpty(uploadedUrl))
                {
                    int customerId = (int)Session["CustomerId"];
                    var customer = db.Customers.Find(customerId);
                    if (customer != null)
                    {
                        customer.AvatarUrl = uploadedUrl;
                        db.SaveChanges();

                        string resolvedUrl = PandoraWeb.Helpers.ImageHelper.GetImageUrl(uploadedUrl, "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?q=80&w=200&auto=format&fit=crop");
                        Session["AvatarUrl"] = resolvedUrl;

                        return Json(new { success = true, avatarUrl = resolvedUrl, message = "Đã tải lên ảnh đại diện thành công!" });
                    }
                }
                return Json(new { success = false, message = "Không thể lưu hình ảnh." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Signup()
        {
            ViewBag.ActiveMenu = "Signup";
            ViewBag.Title = "Đăng Ký";
            return View();
        }

        [HttpPost]
        public ActionResult Signup(string lastName, string firstName, string email, string phone, string password, string confirmPassword)
        {
            ViewBag.ActiveMenu = "Signup";
            ViewBag.Title = "Đăng Ký";

            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            // Check if email already exists
            var existingCustomer = db.Customers.FirstOrDefault(c => c.Email == email);
            var existingEmployee = db.Employees.FirstOrDefault(e => e.Email == email);

            if (existingCustomer != null || existingEmployee != null)
            {
                ViewBag.Error = "Email này đã được sử dụng. Vui lòng chọn email khác.";
                return View();
            }

            // Create new customer
            var customer = new Customer
            {
                FullName = (lastName + " " + firstName).Trim(),
                Email = email,
                PhoneNumber = phone,
                PasswordHash = password,
                Status = "active",
                CreatedAt = System.DateTime.Now
            };

            db.Customers.Add(customer);
            db.SaveChanges();

            // Auto login after signup
            Session["CustomerId"] = customer.CustomerId;
            Session["FullName"] = customer.FullName;
            Session["Role"] = "Customer";
            
            SyncDbCartToSession(customer.CustomerId);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult ChangePassword()
        {
            if (Session["CustomerId"] == null && Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login");
            }
            ViewBag.ActiveMenu = "ChangePassword";
            ViewBag.Title = "Đổi Mật Khẩu";
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (Session["CustomerId"] == null && Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login");
            }
            ViewBag.ActiveMenu = "ChangePassword";
            ViewBag.Title = "Đổi Mật Khẩu";

            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ các trường thông tin.";
                return View();
            }

            if (newPassword.Length < 6)
            {
                ViewBag.Error = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu mới và mật khẩu xác nhận không khớp.";
                return View();
            }

            if (Session["CustomerId"] != null)
            {
                int customerId = (int)Session["CustomerId"];
                var customer = db.Customers.Find(customerId);
                if (customer != null)
                {
                    if (customer.PasswordHash != currentPassword)
                    {
                        ViewBag.Error = "Mật khẩu hiện tại không chính xác.";
                        return View();
                    }
                    customer.PasswordHash = newPassword;
                    db.SaveChanges();
                    ViewBag.Success = "Đổi mật khẩu tài khoản thành công!";
                    return View();
                }
            }
            else if (Session["EmployeeId"] != null)
            {
                int empId = (int)Session["EmployeeId"];
                var emp = db.Employees.Find(empId);
                if (emp != null)
                {
                    if (emp.PasswordHash != currentPassword)
                    {
                        ViewBag.Error = "Mật khẩu hiện tại không chính xác.";
                        return View();
                    }
                    emp.PasswordHash = newPassword;
                    db.SaveChanges();
                    ViewBag.Success = "Đổi mật khẩu tài khoản quản trị thành công!";
                    return View();
                }
            }

            return RedirectToAction("Login");
        }

        public ActionResult ForgotPassword()
        {
            ViewBag.ActiveMenu = "ForgotPassword";
            ViewBag.Title = "Quên Mật Khẩu";
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string loginId, string newPassword, string confirmPassword)
        {
            ViewBag.ActiveMenu = "ForgotPassword";
            ViewBag.Title = "Quên Mật Khẩu";

            if (string.IsNullOrWhiteSpace(loginId))
            {
                ViewBag.Error = "Vui lòng nhập Email hoặc Số điện thoại tài khoản của bạn.";
                return View();
            }

            string target = loginId.Trim();
            var customer = db.Customers.FirstOrDefault(c => c.Email == target || c.PhoneNumber == target);
            var employee = db.Employees.FirstOrDefault(e => e.Email == target);

            if (customer == null && employee == null)
            {
                ViewBag.Error = "Không tìm thấy tài khoản tương ứng với thông tin bạn đã nhập.";
                ViewBag.LoginId = target;
                return View();
            }

            string accName = customer != null ? customer.FullName : employee.FullName;
            ViewBag.AccountFound = true;
            ViewBag.LoginId = target;
            ViewBag.AccountName = accName;

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                return View();
            }

            if (newPassword.Length < 6)
            {
                ViewBag.Error = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            if (customer != null)
            {
                customer.PasswordHash = newPassword;
            }
            else if (employee != null)
            {
                employee.PasswordHash = newPassword;
            }

            db.SaveChanges();
            TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập với mật khẩu mới.";
            return RedirectToAction("Login");
        }

        public ActionResult Address()
        {
            ViewBag.ActiveMenu = "Address";
            ViewBag.Title = "Địa Chỉ Giao Hàng";
            return View();
        }

        private void SyncDbCartToSession(int customerId)
        {
            var dbCart = db.Carts.Include("CartItems.Variant.Product").FirstOrDefault(c => c.CustomerId == customerId);
            var sessionCart = Session["Cart"] as List<CartItemVM> ?? new List<CartItemVM>();

            if (dbCart != null)
            {
                foreach (var dbItem in dbCart.CartItems)
                {
                    var existing = sessionCart.FirstOrDefault(x => x.ProductId == dbItem.Variant.ProductId && x.VariantId == dbItem.VariantId);
                    if (existing == null)
                    {
                        var product = dbItem.Variant.Product;
                        string sizeStr = "", materialStr = "";
                        if (dbItem.Variant.SizeId.HasValue) sizeStr = db.Sizes.Find(dbItem.Variant.SizeId)?.SizeValue;
                        if (dbItem.Variant.MaterialId.HasValue) materialStr = db.Materials.Find(dbItem.Variant.MaterialId)?.MaterialName;

                        sessionCart.Add(new CartItemVM
                        {
                            ProductId = product.ProductId,
                            VariantId = dbItem.VariantId,
                            ProductName = product.ProductName,
                            ImageUrl = product.ImageUrl,
                            Price = product.BasePrice + dbItem.Variant.PriceAdjustment,
                            Quantity = dbItem.Quantity,
                            Size = sizeStr,
                            Material = materialStr
                        });
                    }
                }
            }
            Session["Cart"] = sessionCart;

            // Đồng thời lưu ngược những thứ có sẵn trong session (trước khi login) vào DB
            var currentCart = db.Carts.Include("CartItems").FirstOrDefault(c => c.CustomerId == customerId);
            if (currentCart == null)
            {
                currentCart = new Cart { CustomerId = customerId, CreatedDate = System.DateTime.Now };
                db.Carts.Add(currentCart);
                db.SaveChanges();
            }

            var oldItems = db.CartItems.Where(i => i.CartId == currentCart.CartId).ToList();
            db.CartItems.RemoveRange(oldItems);
            db.SaveChanges();

            foreach (var item in sessionCart)
            {
                db.CartItems.Add(new CartItem
                {
                    CartId = currentCart.CartId,
                    VariantId = item.VariantId,
                    Quantity = item.Quantity
                });
            }
            db.SaveChanges();
        }
    }
}
