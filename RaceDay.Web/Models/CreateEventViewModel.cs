using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RaceDay.Web.Models
{
    public class CreateEventViewModel
    {
        [Required]
        [MaxLength(150)]
        public string EventName { get; set; } = string.Empty;

        public string? EventDescription { get; set; }

        [Required]
        public int EnvironmentID { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public TimeSpan EventTime { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Range(1, 100000)]
        public int MaxParticipants { get; set; }

        [Range(0, 1000000)]
        public decimal EntryFee { get; set; }

        public List<SelectListItem> Environments { get; set; } = new();
    }
}