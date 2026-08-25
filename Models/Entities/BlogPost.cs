using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models
{
    [Table("BlogPosts")]
    public class BlogPost
    {
        [Key]
        public int PostId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; }

        [Required]
        public string Content { get; set; }

        [StringLength(100)]
        public string Author { get; set; }

        public bool IsPublished { get; set; }

        public DateTime PublishedDate { get; set; }
    }
}
