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
    public class SettingsController : Controller
    {
        private PandoraDbContext db = new PandoraDbContext();

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
