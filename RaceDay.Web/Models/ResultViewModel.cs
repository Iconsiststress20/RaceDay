namespace RaceDay.Web.Models
{
    public class ResultViewModel
    {
        public int ResultID { get; set; }

        public int EnrollmentID { get; set; }

        public string EventName { get; set; } = string.Empty;

        public string ParticipantName { get; set; } = string.Empty;

        public int? RaceNumber { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public int? OverallPosition { get; set; }

        public int? CategoryPosition { get; set; }

        public int? Points { get; set; }

        public string? Remarks { get; set; }

        public DateTime RecordedAt { get; set; }

        // Used to populate the enrolment dropdown
        public List<EnrolmentViewModel> AvailableEnrolments { get; set; }
            = new List<EnrolmentViewModel>();
    }
}