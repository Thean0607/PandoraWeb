using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models
{
    [Table("Promotions")]
    public class Promotion
    {
        [Key]
        public int PromotionId { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }

        public int? DiscountPercentage { get; set; }

        [Column(TypeName = "decimal")]
        public decimal? DiscountAmount { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
