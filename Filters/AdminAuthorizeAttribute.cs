using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace PandoraWeb.Filters
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public string Permission { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;

            // Kiểm tra xem đã đăng nhập với tư cách Employee chưa
            if (session["EmployeeId"] == null || session["Role"] == null)
            {
                // Nếu chưa, chuyển hướng về trang Đăng nhập
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Account" },
                        { "action", "Login" }
                    });
                return;
            }

            // Lấy chuỗi permissions từ Session
            string permissions = session["Permissions"]?.ToString() ?? "";
            var permsArray = permissions.Split(',');

            // Nếu người dùng có quyền "all", bỏ qua tất cả kiểm tra và cho phép
            if (permsArray.Contains("all"))
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            // Nếu Action/Controller có yêu cầu quyền cụ thể
            if (!string.IsNullOrEmpty(Permission))
            {
                if (!permsArray.Contains(Permission))
                {
                    // Chuyển hướng về trang chủ Admin kèm thông báo lỗi truy cập
                    filterContext.Controller.TempData["Error"] = "Bạn không có quyền thực hiện chức năng này.";
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary
                        {
                            { "controller", "Admin" },
                            { "action", "Index" }
                        });
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
