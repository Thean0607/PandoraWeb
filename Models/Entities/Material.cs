using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models
{
    [Table("Materials")]
    public class Material
    {
        [Key]
        public int MaterialId { get; set; }

        [Required]
        [StringLength(100)]
        public string MaterialName { get; set; }

        public virtual ICollection<ProductVariant> ProductVariants { get; set; }
    }
}
