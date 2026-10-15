
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    [Table("Participants")]
    public class Participant
    {
        [Key]
        public int ParticipantID { get; set; }

        public int UserID { get; set; }
    }
}
