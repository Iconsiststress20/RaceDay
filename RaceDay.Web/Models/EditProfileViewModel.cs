
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{
    public class EditProfileViewModel
    {
        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        public string? PhoneNumber { get; set; }
    }
}
