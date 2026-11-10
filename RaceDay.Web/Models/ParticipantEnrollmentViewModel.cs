

using Microsoft.AspNetCore.Mvc.Rendering;

namespace RaceDay.Web.Models
{
    

    public class ParticipantEnrollmentViewModel
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = "";
        public int CategoryID { get; set; }
        public List<SelectListItem> Categories { get; set; } = new();
    }

    public class MyEnrollmentViewModel
    {
        public int EnrollmentID { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; } = "";
        public DateTime EventDate { get; set; }
        public TimeSpan EventTime { get; set; }
        public string Location { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public decimal DistanceKM { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; } = "";
        public string PaymentStatus { get; set; } = "";
        public int? RaceNumber { get; set; }
    }
}
