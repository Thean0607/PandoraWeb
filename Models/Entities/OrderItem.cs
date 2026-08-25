using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models
{
    [Table("OrderItems")]
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }

        public int VariantId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal")]
        public decimal UnitPrice { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [ForeignKey("VariantId")]
        public virtual ProductVariant Variant { get; set; }
    }
}
