using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models
{
    [Table("ProductVariants")]
    public class ProductVariant
    {
        [Key]
        public int VariantId { get; set; }

        public int ProductId { get; set; }

        public int? SizeId { get; set; }

        public int? MaterialId { get; set; }

        [StringLength(100)]
        public string SKU { get; set; }

        public int Stock { get; set; }

        [Column(TypeName = "decimal")]
        public decimal PriceAdjustment { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("SizeId")]
        public virtual Size Size { get; set; }

        [ForeignKey("MaterialId")]
        public virtual Material Material { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}
