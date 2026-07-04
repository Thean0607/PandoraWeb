using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models
{
    [Table("Sizes")]
    public class Size
    {
        [Key]
        public int SizeId { get; set; }

        [Required]
        [StringLength(50)]
        public string SizeValue { get; set; }

        public virtual ICollection<ProductVariant> ProductVariants { get; set; }
    }
}
