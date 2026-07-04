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
            return View(cart);
        }

        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity = 1, int? variantId = null)
        {
            var product = db.Products.Find(productId);
            if (product == null) return Json(new { success = false, message = "Sản phẩm không tồn tại" });

            var variant = variantId.HasValue 
                ? db.ProductVariants.Find(variantId.Value)
                : db.ProductVariants.FirstOrDefault(v => v.ProductId == productId);

            int finalVariantId = variant?.VariantId ?? 0;
            decimal price = product.BasePrice + (variant?.PriceAdjustment ?? 0);
            
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
            return Json(new { success = true, totalItems = cart.Sum(x => x.Quantity) });
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
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult UpdateQuantity(int productId, int variantId, int quantity)
        {
            var cart = Session["Cart"] as List<CartItemVM>;
            if (cart != null)
            {
                var item = cart.FirstOrDefault(x => x.ProductId == productId && x.VariantId == variantId);
                if (item != null) item.Quantity = quantity;
                Session["Cart"] = cart;
            }
            return Json(new { success = true });
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
            if (cart == null || !cart.Any()) return RedirectToAction("Cart");
            
            //
            var customer = db.Customers.FirstOrDefault(c => c.Email == email);
            if (customer == null)
            {
                customer = new Customer
                {
                    FullName = fullName,
                    Email = email,
                    PhoneNumber = phone,
                    CreatedAt = DateTime.Now,
                    Status = "active",
                    PasswordHash = "guest"
                };
                db.Customers.Add(customer);
                db.SaveChanges();
            }

            var newAddress = new Address
            {
                CustomerId = customer.CustomerId,
                ReceiverName = fullName,
                PhoneNumber = phone,
                StreetAddress = address,
                City = "Thành phố",
                District = "Quận/Huyện",
                Ward = "Phường/Xã",
                IsDefault = true
            };
            db.Addresses.Add(newAddress);
            db.SaveChanges();

            var order = new Order
            {
                CustomerId = customer.CustomerId,
                ShippingAddressId = newAddress.AddressId,
                TotalAmount = cart.Sum(c => c.Total),
                ShippingFee = 0,
                DiscountAmount = 0,
                OrderStatus = "Pending",
                PaymentMethod = paymentMethod ?? "COD",
                PaymentStatus = "Pending",
                Notes = notes,
                OrderDate = DateTime.Now
            };
            db.Orders.Add(order);
            db.SaveChanges();

            foreach(var item in cart)
            {
                db.OrderItems.Add(new OrderItem
                {
                    OrderId = order.OrderId,
                    VariantId = item.VariantId == 0 ? db.ProductVariants.FirstOrDefault(v => v.ProductId == item.ProductId)?.VariantId ?? 1 : item.VariantId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                });
                
                var variantInDb = db.ProductVariants.Find(item.VariantId);
                if (variantInDb != null) {
                    variantInDb.Stock -= item.Quantity;
                }
            }
            db.SaveChanges();

            Session["Cart"] = null;
            return RedirectToAction("OrderSuccess");
        }

        public ActionResult OrderSuccess()
        {
            ViewBag.ActiveMenu = "OrderSuccess";
            ViewBag.Title = "Đặt Hàng Thành Công";
            return View();
        }

        public ActionResult Orders()
        {
            ViewBag.ActiveMenu = "Orders";
            ViewBag.Title = "Lịch Sử Đơn Hàng";
            return View();
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

