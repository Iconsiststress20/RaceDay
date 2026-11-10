
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{
    public class ProfilePictureViewModel
    {
        [Required]
        public IFormFile? Picture { get; set; }
    }
}
