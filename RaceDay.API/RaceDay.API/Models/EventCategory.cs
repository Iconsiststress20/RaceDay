using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    [Table("EventCategories")]
    public class EventCategory
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        [MaxLength(150)]
        public string CategoryName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal DistanceKM { get; set; }

        public int? MinAge { get; set; }

        public int? MaxAge { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}