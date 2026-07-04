using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models
{
    [Table("Employees")]
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(150)]
        public string FullName { get; set; }

        [Required]
        [StringLength(150)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        public int RoleId { get; set; }

        [StringLength(20)]
        public string Status { get; set; } // active, inactive

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }
    }
}
