
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models
{
    public class OrganiserRegisterRequest
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(12)]
        [MaxLength(128)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string OrganisationName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string ContactEmail { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? ContactPhone { get; set; }
    }
}
