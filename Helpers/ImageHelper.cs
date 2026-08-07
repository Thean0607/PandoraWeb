using System;
using System.Web;
using System.Web.Mvc;

namespace PandoraWeb.Helpers
{
    public static class ImageHelper
    {
        public static string GetImageUrl(string imageUrl, string defaultPath = "~/assets/img/collections/default.jpg")
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return UrlHelper.GenerateContentUrl(defaultPath, new HttpContextWrapper(HttpContext.Current));
            }

            imageUrl = imageUrl.Trim();

            if (imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                imageUrl.StartsWith("//", StringComparison.OrdinalIgnoreCase))
            {
                return imageUrl;
            }

            if (imageUrl.StartsWith("~/"))
            {
                return UrlHelper.GenerateContentUrl(imageUrl, new HttpContextWrapper(HttpContext.Current));
            }

            if (imageUrl.StartsWith("/"))
            {
                return UrlHelper.GenerateContentUrl("~" + imageUrl, new HttpContextWrapper(HttpContext.Current));
            }

            return UrlHelper.GenerateContentUrl("~/" + imageUrl, new HttpContextWrapper(HttpContext.Current));
        }
    }
}
