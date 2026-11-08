using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{
    public class EventCategoryViewModel
    {
        public int CategoryID { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0.1, 1000)]
        [Display(Name = "Distance (KM)")]
        public decimal DistanceKM { get; set; }

        [Display(Name = "Minimum Age")]
        public int? MinAge { get; set; }

        [Display(Name = "Maximum Age")]
        public int? MaxAge { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}