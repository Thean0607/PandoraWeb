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
    public class MarketingController : Controller
    {
        private PandoraDbContext db = new PandoraDbContext();

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
            PandoraWeb.Helpers.ProductHelper.RevertExpiredFlashSales(db);
            var sales = db.Products.Include(p => p.Category).Where(p => p.OldPrice != null && p.OldPrice > p.BasePrice).ToList();
            return View(sales);
        }

        [AdminAuthorize(Permission = "manage_marketing")]
        public ActionResult CreateFlashSale()
        {
            ViewBag.ActiveMenu = "Marketing";
            ViewBag.ActiveSubMenu = "FlashSales";
            ViewBag.Title = "Quản lý Giá Flash Sale Hàng Loạt";
            var products = db.Products.Include(p => p.Category).Where(p => p.Status == "active").OrderByDescending(p => p.ProductId).ToList();
            return View(products);
        }

        public class FlashSaleInput
        {
            public int ProductId { get; set; }
            public int? DiscountPercent { get; set; }
            public decimal? DiscountAmount { get; set; }
            public DateTime? EndDate { get; set; }
        }

        [HttpPost]
        [AdminAuthorize(Permission = "manage_marketing")]
        public ActionResult SaveFlashSale(System.Collections.Generic.List<FlashSaleInput> flashSales)
        {
            if (flashSales == null || !flashSales.Any())
            {
                return Json(new { success = false, message = "Không có dữ liệu gửi lên." });
            }

            foreach (var item in flashSales)
            {
                var p = db.Products.Find(item.ProductId);
                if (p != null)
                {
                    if (item.DiscountPercent.HasValue || item.DiscountAmount.HasValue)
                    {
                        if (!p.OldPrice.HasValue)
                        {
                            p.OldPrice = p.BasePrice;
                        }

                        decimal originalPrice = p.OldPrice.Value;
                        decimal newPrice = originalPrice;

                        if (item.DiscountPercent.HasValue && item.DiscountPercent.Value > 0)
                        {
                            newPrice = originalPrice * (100 - item.DiscountPercent.Value) / 100m;
                        }
                        else if (item.DiscountAmount.HasValue && item.DiscountAmount.Value > 0)
                        {
                            newPrice = originalPrice - item.DiscountAmount.Value;
                        }

                        if (newPrice < 0) newPrice = 0;
                        p.BasePrice = newPrice;
                        p.FlashSaleEndDate = item.EndDate;
                    }
                }
            }
            db.SaveChanges();
            
            TempData["Success"] = "Đã cập nhật Flash Sale thành công!";
            return Json(new { success = true });
        }

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
