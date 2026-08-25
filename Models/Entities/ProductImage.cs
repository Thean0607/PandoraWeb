using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models
{
    [Table("ProductImages")]
    public class ProductImage
    {
        [Key]
        public int ImageId { get; set; }

        public int ProductId { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; }

        public bool IsPrimary { get; set; }

        public int DisplayOrder { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
