using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    [Table("Results")]
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        public int EnrollmentID { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public int? OverallPosition { get; set; }

        public int? CategoryPosition { get; set; }

        public int? Points { get; set; }

        public string? Remarks { get; set; }

        public DateTime RecordedAt { get; set; }
    }
}