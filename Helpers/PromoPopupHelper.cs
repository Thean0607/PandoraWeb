using System;
using System.IO;
using System.Web;
using Newtonsoft.Json;

namespace PandoraWeb.Helpers
{
    public class PromoPopupSettings
    {
        public bool IsEnabled { get; set; } = true;
        public string Title { get; set; } = "ƯU ĐÃI ĐẶC BIỆT THÁNG NÀY";
        public string Subtitle { get; set; } = "Giảm ngay 20% cho đơn hàng trang sức Pandora";
        public string Content { get; set; } = "Nhập mã PANDORA20 khi thanh toán để nhận ngay ưu đãi giảm 20% cùng quà tặng hấp dẫn!";
        public string CouponCode { get; set; } = "PANDORA20";
        public string ImageUrl { get; set; } = "https://images.unsplash.com/photo-1535632066927-ab7c9ab60908?q=80&w=800&auto=format&fit=crop";
        public string ButtonText { get; set; } = "KHÁM PHÁ NGAY";
        public string ButtonLink { get; set; } = "/Product/Category";
        public string BackgroundColor { get; set; } = "#121212";
        public string TextColor { get; set; } = "#FFFFFF";
        public string PopupLayout { get; set; } = "horizontal";
    }

    public static class PromoPopupHelper
    {
        private static string GetFilePath()
        {
            string folder = HttpContext.Current != null
                ? HttpContext.Current.Server.MapPath("~/App_Data/")
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return Path.Combine(folder, "promo_popup.json");
        }

        public static PromoPopupSettings GetSettings()
        {
            try
            {
                string path = GetFilePath();
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var settings = JsonConvert.DeserializeObject<PromoPopupSettings>(json);
                    if (settings != null) return settings;
                }
            }
            catch { }

            var defaultSettings = new PromoPopupSettings();
            SaveSettings(defaultSettings);
            return defaultSettings;
        }

        public static bool SaveSettings(PromoPopupSettings settings)
        {
            try
            {
                string path = GetFilePath();
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(path, json);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
