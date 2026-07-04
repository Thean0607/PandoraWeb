using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models
{
    [Table("Banners")]
    public class Banner
    {
        [Key]
        public int BannerId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; }

        [StringLength(500)]
        public string LinkUrl { get; set; }

        public bool IsActive { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
