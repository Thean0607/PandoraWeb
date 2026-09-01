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
    public class CheckoutController : Controller
    {
        private PandoraDbContext db = new PandoraDbContext();

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

        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "Checkout";
            ViewBag.Title = "Thanh Toán";
            var cart = Session["Cart"] as List<CartItemVM>;
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            foreach (var item in cart)
            {
                int targetVariantId = item.VariantId;
                if (targetVariantId == 0)
                {
                    var v = db.ProductVariants.FirstOrDefault(x => x.ProductId == item.ProductId);
                    if (v != null) targetVariantId = v.VariantId;
                }

                var variant = db.ProductVariants.Find(targetVariantId);
                if (variant == null || variant.Stock < item.Quantity)
                {
                    TempData["ErrorMessage"] = $"Sản phẩm '{item.ProductName}' không đủ số lượng trong kho (chỉ còn {variant?.Stock ?? 0} sản phẩm). Vui lòng cập nhật lại giỏ hàng.";
                    return RedirectToAction("Index", "Cart");
                }

                var prod = db.Products.Find(item.ProductId);
                if (prod != null)
                {
                    item.IsFlashSale = prod.OldPrice.HasValue && prod.BasePrice < prod.OldPrice.Value && (!prod.FlashSaleEndDate.HasValue || prod.FlashSaleEndDate.Value >= DateTime.Now);
                    item.Price = prod.BasePrice + (variant?.PriceAdjustment ?? 0m);
                }
            }

            if (Session["CustomerId"] != null)
            {
                int customerId = (int)Session["CustomerId"];
                ViewBag.Customer = db.Customers.Find(customerId);
                ViewBag.Address = db.Addresses.FirstOrDefault(a => a.CustomerId == customerId && a.IsDefault);
            }

            return View(cart);
        }

        [HttpPost]
        public ActionResult Index(string fullName, string phone, string email, string address, string notes, string paymentMethod, string city = null, string district = null, string ward = null, string street = null)
        {
            var cart = Session["Cart"] as List<CartItemVM>;
            if (cart == null || !cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            // Construct & Validate Address
            string fullAddress = address != null ? address.Trim() : "";

            if (!string.IsNullOrWhiteSpace(street) && !string.IsNullOrWhiteSpace(ward) && !string.IsNullOrWhiteSpace(district) && !string.IsNullOrWhiteSpace(city))
            {
                fullAddress = $"{street.Trim()}, {ward.Trim()}, {district.Trim()}, {city.Trim()}";
            }

            // Strict Validation: Full name, Phone, Email, and Complete Address (must have minimum length and contain components)
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(fullAddress) || fullAddress.Length < 12 || !fullAddress.Contains(","))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ các cấp địa chỉ bắt buộc (Số nhà/Đường/Thôn/Ấp, Phường/Xã, Quận/Huyện, Tỉnh/Thành phố).";
                return RedirectToAction("Index", "Checkout");
            }

            Customer customer = null;
            if (Session["CustomerId"] != null)
            {
                int loggedInCustomerId = (int)Session["CustomerId"];
                customer = db.Customers.Find(loggedInCustomerId);
            }

            if (customer == null)
            {
                string cleanEmail = email.Trim().ToLower();
                customer = db.Customers.FirstOrDefault(c => c.Email.ToLower() == cleanEmail);
                if (customer == null)
                {
                    customer = new Customer
                    {
                        FullName = fullName.Trim(),
                        Email = email.Trim(),
                        PhoneNumber = phone.Trim(),
                        CreatedAt = DateTime.Now,
                        Status = "active",
                        PasswordHash = "guest"
                    };
                    db.Customers.Add(customer);
                    db.SaveChanges();
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(customer.PhoneNumber))
                    {
                        customer.PhoneNumber = phone.Trim();
                        db.SaveChanges();
                    }
                }
            }

            string finalCity = !string.IsNullOrWhiteSpace(city) ? city.Trim() : "Thành phố";
            string finalDistrict = !string.IsNullOrWhiteSpace(district) ? district.Trim() : "Quận/Huyện";
            string finalWard = !string.IsNullOrWhiteSpace(ward) ? ward.Trim() : "Phường/Xã";

            var parts = fullAddress.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 4)
            {
                if (string.IsNullOrWhiteSpace(city)) finalCity = parts[parts.Length - 1].Trim();
                if (string.IsNullOrWhiteSpace(district)) finalDistrict = parts[parts.Length - 2].Trim();
                if (string.IsNullOrWhiteSpace(ward)) finalWard = parts[parts.Length - 3].Trim();
            }

            var newAddress = new Address
            {
                CustomerId = customer.CustomerId,
                ReceiverName = fullName.Trim(),
                PhoneNumber = phone.Trim(),
                StreetAddress = fullAddress,
                City = finalCity,
                District = finalDistrict,
                Ward = finalWard,
                IsDefault = true
            };
            db.Addresses.Add(newAddress);
            db.SaveChanges();

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Re-verify stock inside transaction
                    foreach (var item in cart)
                    {
                        int targetVariantId = item.VariantId;
                        if (targetVariantId == 0)
                        {
                            var v = db.ProductVariants.FirstOrDefault(x => x.ProductId == item.ProductId);
                            if (v != null) targetVariantId = v.VariantId;
                        }

                        var variantInDb = db.ProductVariants.Find(targetVariantId);
                        if (variantInDb == null || variantInDb.Stock < item.Quantity)
                        {
                            transaction.Rollback();
                            TempData["ErrorMessage"] = $"Sản phẩm '{item.ProductName}' không đủ số lượng trong kho (chỉ còn {variantInDb?.Stock ?? 0} sản phẩm). Vui lòng cập nhật giỏ hàng.";
                            return RedirectToAction("Index", "Cart");
                        }
                        
                        var prod = db.Products.Find(item.ProductId);
                        if (prod != null)
                        {
                            item.IsFlashSale = prod.OldPrice.HasValue && prod.BasePrice < prod.OldPrice.Value && (!prod.FlashSaleEndDate.HasValue || prod.FlashSaleEndDate.Value >= DateTime.Now);
                            item.Price = prod.BasePrice + (variantInDb?.PriceAdjustment ?? 0m);
                        }
                    }

                    // 1.5 Calculate Coupon Discount
                    decimal discountAmt = 0m;
                    int? promoId = null;
                    var promo = Session["Coupon"] as PandoraWeb.Models.Promotion;
                    if (promo != null)
                    {
                        // Ensure single usage
                        bool used = db.Orders.Any(o => o.CustomerId == customer.CustomerId && o.PromotionId == promo.PromotionId && o.OrderStatus != "Cancelled");
                        if (!used && promo.IsActive && promo.StartDate <= DateTime.Now && promo.EndDate >= DateTime.Now)
                        {
                            decimal baseDiscountable = cart.Where(m => !m.IsFlashSale).Sum(m => m.Total);
                            if (promo.DiscountPercentage.HasValue && promo.DiscountPercentage.Value > 0)
                            {
                                discountAmt = baseDiscountable * ((decimal)promo.DiscountPercentage.Value / 100);
                            }
                            else if (promo.DiscountAmount.HasValue)
                            {
                                discountAmt = promo.DiscountAmount.Value;
                                if (discountAmt > baseDiscountable) discountAmt = baseDiscountable;
                            }
                            promoId = promo.PromotionId;
                        }
                    }

                    // 2. Create Order
                    var order = new Order
                    {
                        CustomerId = customer.CustomerId,
                        ShippingAddressId = newAddress.AddressId,
                        TotalAmount = cart.Sum(c => c.Total) - discountAmt,
                        ShippingFee = 0m,
                        DiscountAmount = discountAmt,
                        PromotionId = promoId,
                        OrderStatus = "Pending",
                        PaymentMethod = string.IsNullOrEmpty(paymentMethod) ? "COD" : paymentMethod,
                        PaymentStatus = "Pending",
                        Notes = notes ?? "",
                        OrderDate = DateTime.Now
                    };
                    db.Orders.Add(order);
                    db.SaveChanges();

                    // 3. Create OrderItems & Decrement Stock
                    foreach (var item in cart)
                    {
                        int targetVariantId = item.VariantId;
                        if (targetVariantId == 0)
                        {
                            var v = db.ProductVariants.FirstOrDefault(x => x.ProductId == item.ProductId);
                            if (v != null) targetVariantId = v.VariantId;
                        }

                        var orderItem = new OrderItem
                        {
                            OrderId = order.OrderId,
                            VariantId = targetVariantId,
                            Quantity = item.Quantity,
                            UnitPrice = item.Price
                        };
                        db.OrderItems.Add(orderItem);

                        var variantInDb = db.ProductVariants.Find(targetVariantId);
                        if (variantInDb != null)
                        {
                            variantInDb.Stock -= item.Quantity;
                        }
                    }
                    db.SaveChanges();

                    transaction.Commit();
                    
                    PandoraWeb.Helpers.LogHelper.LogActivity("Customer", customer.CustomerId, "CREATE_ORDER", $"Khách hàng đặt thành công đơn hàng {order.OrderId}");

                    // 4. Auto login guest session so user can view order history immediately
                    Session["CustomerId"] = customer.CustomerId;
                    Session["FullName"] = customer.FullName;
                    Session["CustomerEmail"] = customer.Email;
                    Session["Role"] = "Customer";

                    // Clear Session Cart, Coupon & DB Cart
                    Session["Cart"] = null;
                    Session["Coupon"] = null;
                    SaveCartToDb(customer.CustomerId, new List<CartItemVM>());

                    // Send order confirmation email asynchronously to the email address specified at checkout
                    string recipientEmail = !string.IsNullOrWhiteSpace(email) ? email.Trim() : customer?.Email;
                    string recipientName = !string.IsNullOrWhiteSpace(fullName) ? fullName.Trim() : customer?.FullName;

                    if (!string.IsNullOrEmpty(recipientEmail))
                    {
                        PandoraWeb.Helpers.EmailHelper.SendOrderConfirmationEmail(order, recipientEmail, recipientName);
                    }

                    return RedirectToAction("OrderSuccess", new { id = order.OrderId });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý đơn hàng: " + ex.Message;
                    return RedirectToAction("Index", "Cart");
                }
            }
        }

        public ActionResult OrderSuccess(int? id)
        {
            ViewBag.ActiveMenu = "OrderSuccess";
            ViewBag.Title = "Đặt Hàng Thành Công";

            Order order = null;
            if (id.HasValue)
            {
                order = db.Orders
                    .Include(o => o.ShippingAddress)
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems.Select(i => i.Variant.Product))
                    .FirstOrDefault(o => o.OrderId == id.Value);
            }

            return View(order);
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
