using System.Collections.Generic;
using PandoraWeb.Models;

namespace PandoraWeb.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Banner> Banners { get; set; }
        public IEnumerable<Collection> Collections { get; set; }
        public IEnumerable<Product> TrendingProducts { get; set; }
    }
}
