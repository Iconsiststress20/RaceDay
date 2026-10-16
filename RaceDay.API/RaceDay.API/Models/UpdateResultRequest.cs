
using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models
{
    public class UpdateResultRequest
    {
        public TimeSpan? FinishTime { get; set; }

        [Range(1, int.MaxValue)]
        public int? OverallPosition { get; set; }

        [Range(1, int.MaxValue)]
        public int? CategoryPosition { get; set; }

        [Range(0, int.MaxValue)]
        public int? Points { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }
    }
}
