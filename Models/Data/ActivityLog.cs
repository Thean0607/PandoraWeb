using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models.Data
{
    [Table("ActivityLogs")]
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }

        [StringLength(50)]
        public string UserType { get; set; } // "Customer" or "Employee"

        [StringLength(100)]
        public string Action { get; set; }

        public string Description { get; set; }

        [StringLength(100)]
        public string IpAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
