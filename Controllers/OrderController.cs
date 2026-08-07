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
    public class OrderController : Controller
    {
        private PandoraDbContext db = new PandoraDbContext();

        public ActionResult Cart()
        {
            ViewBag.ActiveMenu = "Cart";
            ViewBag.Title = "Giỏ Hàng";
            
            var cart = Session["Cart"] as List<CartItemVM>;
            if (cart == null)
            {
                cart = new List<CartItemVM>();
            }

            // Kiểm tra tồn kho
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
                    Material = materialStr
                });
            }

            Session["Cart"] = cart;

            if (Session["CustomerId"] != null)
            {
                SaveCartToDb((int)Session["CustomerId"], cart);
            }

            int totalItems = cart.Sum(x => x.Quantity);
            decimal subTotal = cart.Sum(x => x.Total);

            return Json(new { 
                success = true, 
                totalItems = totalItems, 
                subTotal = subTotal.ToString("N0") + " ₫",
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

            return Json(new { 
                success = true, 
                totalItems = totalItems, 
                subTotal = subTotal.ToString("N0") + " ₫",
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

            return Json(new { 
                success = true, 
                totalItems = totalItems, 
                itemTotal = itemTotal.ToString("N0") + " ₫",
                subTotal = subTotal.ToString("N0") + " ₫",
                isEmpty = isEmpty
            });
        }

        public ActionResult Checkout()
        {
            ViewBag.ActiveMenu = "Checkout";
            ViewBag.Title = "Thanh Toán";
            var cart = Session["Cart"] as List<CartItemVM>;
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Cart");
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
                    return RedirectToAction("Cart");
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
        public ActionResult Checkout(string fullName, string phone, string email, string address, string notes, string paymentMethod)
        {
            var cart = Session["Cart"] as List<CartItemVM>;
            if (cart == null || !cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Cart");
            }

            // Input Validation
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(address))
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ các thông tin bắt buộc (Họ tên, SĐT, Email, Địa chỉ).";
                return RedirectToAction("Checkout");
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

            var newAddress = new Address
            {
                CustomerId = customer.CustomerId,
                ReceiverName = fullName.Trim(),
                PhoneNumber = phone.Trim(),
                StreetAddress = address.Trim(),
                City = "Thành phố",
                District = "Quận/Huyện",
                Ward = "Phường/Xã",
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
                            return RedirectToAction("Cart");
                        }
                    }

                    // 2. Create Order
                    var order = new Order
                    {
                        CustomerId = customer.CustomerId,
                        ShippingAddressId = newAddress.AddressId,
                        TotalAmount = cart.Sum(c => c.Total),
                        ShippingFee = 0m,
                        DiscountAmount = 0m,
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

                    // 4. Auto login guest session so user can view order history immediately
                    Session["CustomerId"] = customer.CustomerId;
                    Session["FullName"] = customer.FullName;
                    Session["CustomerEmail"] = customer.Email;
                    Session["Role"] = "Customer";

                    // Clear Session Cart & DB Cart
                    Session["Cart"] = null;
                    SaveCartToDb(customer.CustomerId, new List<CartItemVM>());

                    // Send order confirmation email asynchronously to the buyer's email
                    if (customer != null && !string.IsNullOrEmpty(customer.Email))
                    {
                        PandoraWeb.Helpers.EmailHelper.SendOrderConfirmationEmail(order, customer.Email, customer.FullName);
                    }

                    return RedirectToAction("OrderSuccess", new { id = order.OrderId });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý đơn hàng: " + ex.Message;
                    return RedirectToAction("Cart");
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

        public ActionResult Lookup()
        {
            ViewBag.ActiveMenu = "Lookup";
            ViewBag.Title = "Tra Cứu Đơn Hàng";
            return View();
        }

        [HttpPost]
        public ActionResult Lookup(string orderId, string searchKey)
        {
            ViewBag.ActiveMenu = "Lookup";
            ViewBag.Title = "Tra Cứu Đơn Hàng";
            ViewBag.OrderIdVal = orderId;
            ViewBag.SearchKeyVal = searchKey;

            if (string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(searchKey))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập Mã đơn hàng và Số điện thoại / Email để tra cứu.";
                return View();
            }

            string cleanIdStr = orderId.Replace("#", "").Replace("PAN", "").Replace("pan", "").Trim();
            int id;
            if (!int.TryParse(cleanIdStr, out id))
            {
                ViewBag.ErrorMessage = "Mã đơn hàng không hợp lệ (Ví dụ hợp lệ: #PAN1 hoặc 1).";
                return View();
            }

            string cleanKey = searchKey.Trim().ToLower();

            var order = db.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems.Select(i => i.Variant.Product))
                .FirstOrDefault(o => o.OrderId == id &&
                    (
                        (o.Customer != null && o.Customer.Email.ToLower() == cleanKey) ||
                        (o.Customer != null && o.Customer.PhoneNumber != null && o.Customer.PhoneNumber.Contains(cleanKey)) ||
                        (o.ShippingAddress != null && o.ShippingAddress.PhoneNumber != null && o.ShippingAddress.PhoneNumber.Contains(cleanKey)) ||
                        (o.ShippingAddress != null && o.ShippingAddress.ReceiverName != null && o.ShippingAddress.ReceiverName.ToLower().Contains(cleanKey))
                    ));

            if (order == null)
            {
                ViewBag.ErrorMessage = "Không tìm thấy đơn hàng phù hợp với Mã đơn và SĐT/Email đã nhập.";
                return View();
            }

            return View(order);
        }

        public ActionResult Orders()
        {
            ViewBag.ActiveMenu = "Orders";
            ViewBag.Title = "Lịch Sử Đơn Hàng";

            if (Session["CustomerId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int customerId = (int)Session["CustomerId"];
            ViewBag.Customer = db.Customers.Find(customerId);

            var orders = db.Orders
                .Include(o => o.OrderItems.Select(i => i.Variant.Product))
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        public ActionResult Wishlist()
        {
            ViewBag.ActiveMenu = "Wishlist";
            ViewBag.Title = "Danh Sách Yêu Thích";
            
            var wishlistIds = Session["Wishlist"] as List<int>;
            List<Product> products = new List<Product>();
            
            if (wishlistIds != null && wishlistIds.Any())
            {
                products = db.Products.Include(p => p.Category).Where(p => wishlistIds.Contains(p.ProductId)).ToList();
            }
            
            return View(products);
        }

        [HttpPost]
        public ActionResult AddToWishlist(int productId)
        {
            var wishlist = Session["Wishlist"] as List<int>;
            if (wishlist == null) wishlist = new List<int>();

            if (!wishlist.Contains(productId))
            {
                wishlist.Add(productId);
            }
            Session["Wishlist"] = wishlist;
            return Json(new { success = true, totalItems = wishlist.Count });
        }

        [HttpPost]
        public ActionResult RemoveFromWishlist(int productId)
        {
            var wishlist = Session["Wishlist"] as List<int>;
            if (wishlist != null)
            {
                wishlist.Remove(productId);
                Session["Wishlist"] = wishlist;
            }
            return Json(new { success = true });
        }
    }
}

