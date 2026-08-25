using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandoraWeb.Models
{
    [Table("Addresses")]
    public class Address
    {
        [Key]
        public int AddressId { get; set; }

        public int CustomerId { get; set; }

        [Required]
        [StringLength(150)]
        public string ReceiverName { get; set; }

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(255)]
        public string StreetAddress { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [Required]
        [StringLength(100)]
        public string District { get; set; }

        [Required]
        [StringLength(100)]
        public string Ward { get; set; }

        public bool IsDefault { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }
    }
}
