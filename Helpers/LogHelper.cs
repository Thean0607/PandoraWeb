using System;
using System.Web;
using PandoraWeb.Models.Data;

namespace PandoraWeb.Helpers
{
    public static class LogHelper
    {
        public static void LogActivity(string userType, int? userId, string action, string description)
        {
            try
            {
                using (var db = new PandoraDbContext())
                {
                    string ipAddress = HttpContext.Current?.Request?.UserHostAddress;

                    var log = new ActivityLog
                    {
                        UserType = userType,
                        UserId = userId,
                        Action = action,
                        Description = description,
                        IpAddress = ipAddress,
                        CreatedAt = DateTime.Now
                    };
                    db.ActivityLogs.Add(log);
                    db.SaveChanges();
                }
            }
            catch
            {
                // Ignore logging errors to prevent application crash
            }
        }

        public static void LogError(Exception ex, string source = "")
        {
            try
            {
                using (var db = new PandoraDbContext())
                {
                    string url = HttpContext.Current?.Request?.Url?.ToString();

                    var log = new SystemLog
                    {
                        Message = ex.Message,
                        StackTrace = ex.StackTrace,
                        Source = source,
                        Url = url,
                        CreatedAt = DateTime.Now
                    };
                    db.SystemLogs.Add(log);
                    db.SaveChanges();
                }
            }
            catch
            {
                // Ignore logging errors to prevent application crash
            }
        }
    }
}
