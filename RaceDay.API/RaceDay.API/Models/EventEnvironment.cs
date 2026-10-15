using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    [Table("EventEnvironments")]
    public class EventEnvironment
    {
        [Key]
        public int EnvironmentID { get; set; }

        [Required]
        [MaxLength(150)]
        public string EnvironmentName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string EnvironmentType { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? Province { get; set; }

        public string? RouteInformation { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}