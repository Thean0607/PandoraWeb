using PandoraWeb.Models.Data;
using System;
using System.Linq;

namespace PandoraWeb.Helpers
{
    public static class ProductHelper
    {
        public static void RevertExpiredFlashSales(PandoraDbContext db)
        {
            var expiredProducts = db.Products
                .Where(p => p.FlashSaleEndDate != null && p.FlashSaleEndDate < DateTime.Now)
                .ToList();

            bool changed = false;
            foreach (var p in expiredProducts)
            {
                if (p.OldPrice.HasValue)
                {
                    p.BasePrice = p.OldPrice.Value;
                    p.OldPrice = null;
                }
                p.FlashSaleStartDate = null;
                p.FlashSaleEndDate = null;
                changed = true;
            }

            if (changed)
            {
                db.SaveChanges();
            }
        }
    }
}
