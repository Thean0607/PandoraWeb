using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models
{
    [Table("Pages")]
    public class Page
    {
        [Key]
        public int PageId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(200)]
        public string Slug { get; set; }

        public string Content { get; set; }

        public bool IsPublished { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
