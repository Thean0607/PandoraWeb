using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models.Data
{
    [Table("SystemLogs")]
    public class SystemLog
    {
        [Key]
        public int Id { get; set; }

        public string Message { get; set; }

        public string StackTrace { get; set; }

        [StringLength(500)]
        public string Source { get; set; }

        [StringLength(500)]
        public string Url { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
