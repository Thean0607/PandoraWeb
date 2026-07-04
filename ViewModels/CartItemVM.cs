namespace PandoraWeb.ViewModels
{
    public class CartItemVM
    {
        public int ProductId { get; set; }
        public int VariantId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Size { get; set; }
        public string Material { get; set; }
        public decimal Total => Price * Quantity;
    }
}
