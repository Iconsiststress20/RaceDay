
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganisersController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public OrganisersController(RaceDayContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrganisers()
        {
            var organisers = await _context.Organisers
                .Select(organiser => new
                {
                    organiser.OrganiserID,
                    organiser.UserID,
                    organiser.OrganisationName,
                    organiser.ContactEmail,
                    organiser.ContactPhone
                })
                .ToListAsync();

            return Ok(organisers);
        }
    }
}
