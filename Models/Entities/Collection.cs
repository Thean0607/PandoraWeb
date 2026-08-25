using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models
{
    [Table("Collections")]
    public class Collection
    {
        [Key]
        public int CollectionId { get; set; }

        [Required]
        [StringLength(200)]
        public string CollectionName { get; set; }

        public string Description { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
