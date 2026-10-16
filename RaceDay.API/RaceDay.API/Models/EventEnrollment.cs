using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    [Table("EventEnrollments")]
    public class EventEnrollment
    {
        [Key]
        public int EnrollmentID { get; set; }

        public int ParticipantID { get; set; }

        public int EventID { get; set; }

        public int CategoryID { get; set; }

        public DateTime EnrollmentDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string PaymentStatus { get; set; } = string.Empty;

        public int? RaceNumber { get; set; }
    }
}