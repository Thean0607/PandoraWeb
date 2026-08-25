using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models
{
    [Table("CartItems")]
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

        public int CartId { get; set; }

        public int VariantId { get; set; }

        public int Quantity { get; set; }

        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; }

        [ForeignKey("VariantId")]
        public virtual ProductVariant Variant { get; set; }
    }
}
