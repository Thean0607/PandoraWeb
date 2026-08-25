using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models
{
    [Table("Roles")]
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [StringLength(100)]
        public string RoleName { get; set; }

        // Store permissions as JSON string or comma-separated
        public string Permissions { get; set; }

        // Navigation property
        public virtual ICollection<Employee> Employees { get; set; }
    }
}
