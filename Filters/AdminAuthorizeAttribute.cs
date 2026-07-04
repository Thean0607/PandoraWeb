using System.Web.Mvc;
using System.Web.Routing;

namespace PandoraWeb.Filters
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
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
            }
            else
            {
                // Kiểm tra phân quyền cụ thể hơn nếu cần (ví dụ: chỉ Admin/Manager mới được vào)
                string role = session["Role"].ToString();
                if (role != "Admin" && role != "Manager")
                {
                    // Nếu là nhân viên nhưng không có quyền quản lý, đẩy về trang báo lỗi hoặc trang chủ
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary
                        {
                            { "controller", "Home" },
                            { "action", "Index" }
                        });
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
