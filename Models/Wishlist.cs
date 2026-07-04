using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models
{
    [Table("Wishlists")]
    public class Wishlist
    {
        [Key]
        public int WishlistId { get; set; }

        public int CustomerId { get; set; }

        public int ProductId { get; set; }

        public DateTime AddedDate { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
