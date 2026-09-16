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
        [ValidateAntiForgeryToken]
        public ActionResult Login(string loginId, string password)
        {
            // Kiểm tra trong bảng Employees trước (Admin/Manager)
            var emp = db.Employees.Include("Role").FirstOrDefault(e => e.Email == loginId);
            if (emp != null && PandoraWeb.Helpers.SecurityHelper.VerifyPassword(password, emp.PasswordHash))
            {
                Session["EmployeeId"] = emp.EmployeeId;
                Session["FullName"] = emp.FullName;
                Session["Role"] = emp.Role.RoleName;
                Session["Permissions"] = emp.Role.Permissions;
                PandoraWeb.Helpers.LogHelper.LogActivity("Employee", emp.EmployeeId, "LOGIN_SUCCESS", "Nhân viên đăng nhập thành công");
                return RedirectToAction("Index", "Admin", new { area = "Admin" });
            }

            // Kiểm tra trong bảng Customers (Khách hàng)
            var cus = db.Customers.FirstOrDefault(c => (c.Email == loginId || c.PhoneNumber == loginId));
            if (cus != null && PandoraWeb.Helpers.SecurityHelper.VerifyPassword(password, cus.PasswordHash))
            {
                Session["CustomerId"] = cus.CustomerId;
                Session["FullName"] = cus.FullName;
                Session["Role"] = "Customer";
                if (!string.IsNullOrEmpty(cus.AvatarUrl))
                {
                    Session["AvatarUrl"] = PandoraWeb.Helpers.ImageHelper.GetImageUrl(cus.AvatarUrl, "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?q=80&w=200&auto=format&fit=crop");
                }
                
                SyncDbCartToSession(cus.CustomerId);
                SyncWishlist(cus.CustomerId);
                PandoraWeb.Helpers.LogHelper.LogActivity("Customer", cus.CustomerId, "LOGIN_SUCCESS", "Khách hàng đăng nhập thành công");
                
                return RedirectToAction("Index", "Home");
            }

            PandoraWeb.Helpers.LogHelper.LogActivity("Unknown", null, "LOGIN_FAILED", "Đăng nhập thất bại với tài khoản: {loginId}");
            ViewBag.Error = "Email hoặc mật khẩu không đúng!";
            return View();
        }
