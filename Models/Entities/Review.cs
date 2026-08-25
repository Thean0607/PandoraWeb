using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models
{
    [Table("Reviews")]
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        public int ProductId { get; set; }

        public int CustomerId { get; set; }

        public int Rating { get; set; } // 1-5

        [StringLength(1000)]
        public string Comment { get; set; }

        public DateTime ReviewDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; } // Approved, Pending, Rejected

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }
    }
}
