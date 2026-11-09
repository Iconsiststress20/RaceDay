
namespace RaceDay.Web.Models
{
    public class EnrolmentViewModel
    {
        public int EnrollmentID { get; set; }

        public string ParticipantName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public int ParticipantID { get; set; }

        public int EventID { get; set; }

        public string EventName { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public int CategoryID { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public decimal DistanceKM { get; set; }

        public DateTime EnrollmentDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = string.Empty;

        public int? RaceNumber { get; set; }
    }
}

