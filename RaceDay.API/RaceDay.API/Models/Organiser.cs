
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    [Table("Organisers")]
    public class Organiser
    {
        [Key]
        public int OrganiserID { get; set; }

        public int UserID { get; set; }

        [Required]
        [MaxLength(150)]
        public string OrganisationName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ContactEmail { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? ContactPhone { get; set; }
    }
}
