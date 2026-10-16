
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        [MaxLength(128)]
        public string Password { get; set; } = string.Empty;
    }
}
