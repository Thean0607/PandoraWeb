using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using PandoraWeb.Models;
using PandoraWeb.Models.Data;

namespace PandoraWeb.Controllers
{
    public class WishlistController : Controller
    {
        private PandoraDbContext db = new PandoraDbContext();

        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "Wishlist";
            ViewBag.Title = "Danh Sách Yêu Thích";
            
            List<int> wishlistIds = new List<int>();
            
            if (Session["CustomerId"] != null)
            {
                int customerId = (int)Session["CustomerId"];
                wishlistIds = db.Wishlists.Where(w => w.CustomerId == customerId).Select(w => w.ProductId).ToList();
            }
            else
            {
                wishlistIds = Session["Wishlist"] as List<int> ?? new List<int>();
            }

            List<Product> products = new List<Product>();
            
            if (wishlistIds.Any())
            {
                products = db.Products.Include(p => p.Category).Where(p => wishlistIds.Contains(p.ProductId)).ToList();
            }
            
            return View(products);
        }

        [HttpPost]
        public ActionResult AddToWishlist(int productId)
        {
            if (Session["CustomerId"] != null)
            {
                int customerId = (int)Session["CustomerId"];
                if (!db.Wishlists.Any(w => w.CustomerId == customerId && w.ProductId == productId))
                {
                    db.Wishlists.Add(new Models.Wishlist { CustomerId = customerId, ProductId = productId, AddedDate = System.DateTime.Now });
                    db.SaveChanges();
                }
                int totalItems = db.Wishlists.Count(w => w.CustomerId == customerId);
                return Json(new { success = true, totalItems = totalItems });
            }
            else
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
        }

        [HttpPost]
        public ActionResult RemoveFromWishlist(int productId)
        {
            if (Session["CustomerId"] != null)
            {
                int customerId = (int)Session["CustomerId"];
                var wishlistItem = db.Wishlists.FirstOrDefault(w => w.CustomerId == customerId && w.ProductId == productId);
                if (wishlistItem != null)
                {
                    db.Wishlists.Remove(wishlistItem);
                    db.SaveChanges();
                }
                return Json(new { success = true });
            }
            else
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
