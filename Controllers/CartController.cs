using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using PandoraWeb.Models;
using PandoraWeb.Models.Data;
using PandoraWeb.ViewModels;

namespace PandoraWeb.Controllers
{
    public class CartController : Controller
    {
        private PandoraDbContext db = new PandoraDbContext();

        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "Cart";
            ViewBag.Title = "Giỏ Hàng";
            
            var cart = Session["Cart"] as List<CartItemVM>;
            if (cart == null)
            {
                cart = new List<CartItemVM>();
            }

            // Kiểm tra tồn kho và cập nhật giá/flash sale
            foreach (var item in cart)
            {
                var variant = db.ProductVariants.Find(item.VariantId);
                if (variant == null || variant.Stock < item.Quantity)
                {
                    item.IsOutOfStock = true;
                    ViewBag.HasOutOfStock = true;
                }
                else
                {
                    item.IsOutOfStock = false;
                }

                var prod = db.Products.Find(item.ProductId);
                if (prod != null)
                {
                    item.IsFlashSale = prod.OldPrice.HasValue && prod.BasePrice < prod.OldPrice.Value && (!prod.FlashSaleEndDate.HasValue || prod.FlashSaleEndDate.Value >= DateTime.Now);
                    item.Price = prod.BasePrice + (variant?.PriceAdjustment ?? 0m); // Update price dynamically
                }
            }

            return View(cart);
        }

        private void SaveCartToDb(int customerId, List<CartItemVM> sessionCart)
        {
            var dbCart = db.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.CustomerId == customerId);
            if (dbCart == null)
            {
                dbCart = new Cart { CustomerId = customerId, CreatedDate = DateTime.Now };
                db.Carts.Add(dbCart);
                db.SaveChanges();
            }
            
            var oldItems = db.CartItems.Where(i => i.CartId == dbCart.CartId).ToList();
            db.CartItems.RemoveRange(oldItems);
            db.SaveChanges();

            if (sessionCart != null && sessionCart.Any())
            {
                foreach (var item in sessionCart)
                {
                    db.CartItems.Add(new CartItem
                    {
                        CartId = dbCart.CartId,
                        VariantId = item.VariantId,
                        Quantity = item.Quantity
                    });
                }
                db.SaveChanges();
            }
        }

        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity = 1, int? variantId = null)
        {
            if (quantity <= 0) quantity = 1;
            var product = db.Products.Find(productId);
            if (product == null) return Json(new { success = false, message = "Sản phẩm không tồn tại" });

            var variant = variantId.HasValue 
                ? db.ProductVariants.Find(variantId.Value)
                : db.ProductVariants.FirstOrDefault(v => v.ProductId == productId);

            int finalVariantId = variant?.VariantId ?? 0;
            decimal price = product.BasePrice + (variant?.PriceAdjustment ?? 0m);
            bool isFlashSale = product.OldPrice.HasValue && product.BasePrice < product.OldPrice.Value && (!product.FlashSaleEndDate.HasValue || product.FlashSaleEndDate.Value >= DateTime.Now);
            
            // Lấy thêm thông tin size/material
            string sizeStr = "", materialStr = "";
            if (variant != null) {
                if (variant.SizeId.HasValue) sizeStr = db.Sizes.Find(variant.SizeId)?.SizeValue;
                if (variant.MaterialId.HasValue) materialStr = db.Materials.Find(variant.MaterialId)?.MaterialName;
            }

            var cart = Session["Cart"] as List<CartItemVM>;
            if (cart == null) cart = new List<CartItemVM>();

            var existingItem = cart.FirstOrDefault(x => x.ProductId == productId && x.VariantId == finalVariantId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItemVM
                {
                    ProductId = productId,
                    VariantId = finalVariantId,
                    ProductName = product.ProductName,
                    ImageUrl = product.ImageUrl,
                    Price = price,
                    Quantity = quantity,
                    Size = sizeStr,
                    Material = materialStr,
                    IsFlashSale = isFlashSale
                });
            }

            Session["Cart"] = cart;

            if (Session["CustomerId"] != null)
            {
                SaveCartToDb((int)Session["CustomerId"], cart);
            }

            int totalItems = cart.Sum(x => x.Quantity);
            decimal subTotal = cart.Sum(x => x.Total);
            
            decimal discountAmt = 0m;
            var promo = Session["Coupon"] as PandoraWeb.Models.Promotion;
            if (promo != null)
            {
                decimal baseDiscountable = cart.Where(m => !m.IsFlashSale).Sum(m => m.Total);
                if (promo.DiscountPercentage.HasValue && promo.DiscountPercentage.Value > 0)
                {
                    discountAmt = baseDiscountable * ((decimal)promo.DiscountPercentage.Value / 100);
                }
                else if (promo.DiscountAmount.HasValue)
                {
                    discountAmt = promo.DiscountAmount.Value;
                    if (discountAmt > baseDiscountable) { discountAmt = baseDiscountable; }
                }
            }
            decimal finalTotal = subTotal - discountAmt;

            return Json(new { 
                success = true, 
                totalItems = totalItems, 
                subTotal = subTotal.ToString("N0") + " ₫",
                discountAmt = discountAmt.ToString("N0") + " ₫",
                finalTotal = finalTotal.ToString("N0") + " ₫",
                message = "Đã thêm sản phẩm vào giỏ hàng!"
            });
        }

        [HttpPost]
        public ActionResult RemoveFromCart(int productId, int variantId)
        {
            var cart = Session["Cart"] as List<CartItemVM>;
            if (cart != null)
            {
                var item = cart.FirstOrDefault(x => x.ProductId == productId && x.VariantId == variantId);
                if (item != null) cart.Remove(item);
                Session["Cart"] = cart;

                if (Session["CustomerId"] != null)
                {
                    SaveCartToDb((int)Session["CustomerId"], cart);
                }
            }
            int totalItems = cart != null ? cart.Sum(x => x.Quantity) : 0;
            decimal subTotal = cart != null ? cart.Sum(x => x.Total) : 0m;
            bool isEmpty = cart == null || !cart.Any();

            decimal discountAmt = 0m;
            var promo = Session["Coupon"] as PandoraWeb.Models.Promotion;
            if (promo != null && cart != null)
            {
                decimal baseDiscountable = cart.Where(m => !m.IsFlashSale).Sum(m => m.Total);
                if (promo.DiscountPercentage.HasValue && promo.DiscountPercentage.Value > 0)
                {
                    discountAmt = baseDiscountable * ((decimal)promo.DiscountPercentage.Value / 100);
                }
                else if (promo.DiscountAmount.HasValue)
                {
                    discountAmt = promo.DiscountAmount.Value;
                    if (discountAmt > baseDiscountable) { discountAmt = baseDiscountable; }
                }
            }
            decimal finalTotal = subTotal - discountAmt;

            return Json(new { 
                success = true, 
                totalItems = totalItems, 
                subTotal = subTotal.ToString("N0") + " ₫",
                discountAmt = discountAmt.ToString("N0") + " ₫",
                finalTotal = finalTotal.ToString("N0") + " ₫",
                isEmpty = isEmpty
            });
        }

        [HttpPost]
        public ActionResult UpdateQuantity(int productId, int variantId, int quantity)
        {
            var cart = Session["Cart"] as List<CartItemVM>;
            decimal itemTotal = 0m;
            if (cart != null)
            {
                var item = cart.FirstOrDefault(x => x.ProductId == productId && x.VariantId == variantId);
                if (item != null)
                {
                    if (quantity <= 0)
                    {
                        cart.Remove(item);
                    }
                    else
                    {
                        item.Quantity = quantity;
                        itemTotal = item.Total;
                    }
                }
                Session["Cart"] = cart;

                if (Session["CustomerId"] != null)
                {
                    SaveCartToDb((int)Session["CustomerId"], cart);
                }
            }
            int totalItems = cart != null ? cart.Sum(x => x.Quantity) : 0;
            decimal subTotal = cart != null ? cart.Sum(x => x.Total) : 0m;
            bool isEmpty = cart == null || !cart.Any();

            decimal discountAmt = 0m;
            var promo = Session["Coupon"] as PandoraWeb.Models.Promotion;
            if (promo != null && cart != null)
            {
                decimal baseDiscountable = cart.Where(m => !m.IsFlashSale).Sum(m => m.Total);
                if (promo.DiscountPercentage.HasValue && promo.DiscountPercentage.Value > 0)
                {
                    discountAmt = baseDiscountable * ((decimal)promo.DiscountPercentage.Value / 100);
                }
                else if (promo.DiscountAmount.HasValue)
                {
                    discountAmt = promo.DiscountAmount.Value;
                    if (discountAmt > baseDiscountable) { discountAmt = baseDiscountable; }
                }
            }
            decimal finalTotal = subTotal - discountAmt;

            return Json(new { 
                success = true, 
                totalItems = totalItems, 
                itemTotal = itemTotal.ToString("N0") + " ₫",
                subTotal = subTotal.ToString("N0") + " ₫",
                discountAmt = discountAmt.ToString("N0") + " ₫",
                finalTotal = finalTotal.ToString("N0") + " ₫",
                isEmpty = isEmpty
            });
        }

        [HttpPost]
        public ActionResult ApplyCoupon(string code)
        {
            if (string.IsNullOrEmpty(code))
                return Json(new { success = false, message = "Vui lòng nhập mã." });
            
            var promo = db.Promotions.FirstOrDefault(p => p.Code == code && p.IsActive);
            if (promo == null)
                return Json(new { success = false, message = "Mã giảm giá không hợp lệ hoặc đã hết hạn." });
                
            if (promo.StartDate > DateTime.Now || promo.EndDate < DateTime.Now)
                return Json(new { success = false, message = "Mã giảm giá không trong thời gian sử dụng." });

            int? customerId = Session["CustomerId"] as int?;
            if (customerId.HasValue)
            {
                bool used = db.Orders.Any(o => o.CustomerId == customerId.Value && o.PromotionId == promo.PromotionId && o.OrderStatus != "Cancelled");
                if (used)
                    return Json(new { success = false, message = "Tài khoản của bạn đã sử dụng mã này rồi." });
            }
            else
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để sử dụng mã giảm giá." });
            }

            Session["Coupon"] = promo;
            return Json(new { success = true, message = "Áp dụng mã thành công!" });
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
