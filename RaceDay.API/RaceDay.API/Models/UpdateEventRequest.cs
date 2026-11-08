using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models
{
    public class UpdateEventRequest
    {
        [Required]
        [MaxLength(150)]
        public string EventName { get; set; } = string.Empty;

        public string? EventDescription { get; set; }

        public int EnvironmentID { get; set; }

        public DateTime EventDate { get; set; }

        public TimeSpan EventTime { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        public int MaxParticipants { get; set; }

        public decimal EntryFee { get; set; }

        public string Status { get; set; } = "Upcoming";
    }
}