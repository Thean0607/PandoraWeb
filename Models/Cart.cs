using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models
{
    [Table("Carts")]
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        public int CustomerId { get; set; }

        public DateTime CreatedDate { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}
