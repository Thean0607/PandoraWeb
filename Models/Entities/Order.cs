using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public int CustomerId { get; set; }

        public int? ShippingAddressId { get; set; }

        public int? PromotionId { get; set; }

        [Column(TypeName = "decimal")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal")]
        public decimal ShippingFee { get; set; }

        [Column(TypeName = "decimal")]
        public decimal DiscountAmount { get; set; }

        [StringLength(50)]
        public string OrderStatus { get; set; } // Pending, Processing, Shipped, Delivered, Cancelled

        [StringLength(50)]
        public string PaymentMethod { get; set; } // COD, VNPay, Momo

        [StringLength(50)]
        public string PaymentStatus { get; set; } // Pending, Paid, Failed

        [StringLength(500)]
        public string Notes { get; set; }

        public DateTime OrderDate { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        [ForeignKey("ShippingAddressId")]
        public virtual Address ShippingAddress { get; set; }

        [ForeignKey("PromotionId")]
        public virtual Promotion Promotion { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
