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

        public new ActionResult Profile()
        {
            if (Session["FullName"] == null) return RedirectToAction("Login");
            ViewBag.ActiveMenu = "Profile";
            return View();
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
            ViewBag.ActiveMenu = "ChangePassword";
            ViewBag.Title = "Đổi Mật Khẩu";
            return View();
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
