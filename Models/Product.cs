using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [StringLength(200)]
        public string ProductName { get; set; }

        public string Description { get; set; }

        public int CategoryId { get; set; }

        public int? CollectionId { get; set; }

        [Column(TypeName = "decimal")]
        public decimal BasePrice { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; }

        [StringLength(20)]
        public string Status { get; set; } // active, inactive

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [ForeignKey("CollectionId")]
        public virtual Collection Collection { get; set; }

        public virtual ICollection<ProductVariant> ProductVariants { get; set; }
        public virtual ICollection<ProductImage> ProductImages { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Wishlist> Wishlists { get; set; }
    }
}
