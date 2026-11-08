namespace RaceDay.Web.Models
{
    public class EventViewModel
    {
        public int EventID { get; set; }
        public int OrganiserID { get; set; }
        public int EnvironmentID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string? EventDescription { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan EventTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public int MaxParticipants { get; set; }
        public decimal EntryFee { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}