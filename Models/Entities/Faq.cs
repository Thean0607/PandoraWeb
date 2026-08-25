using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models
{
    [Table("Faqs")]
    public class Faq
    {
        [Key]
        public int FaqId { get; set; }

        [Required]
        [StringLength(500)]
        public string Question { get; set; }

        [Required]
        public string Answer { get; set; }

        public bool IsActive { get; set; }

        public int DisplayOrder { get; set; }
    }
}
