using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;

namespace RaceDay.Controllers
{
    [Authorize(Roles = "Organiser")]
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EventsController(RaceDayContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .Select(raceEvent => new
                {
                    raceEvent.EventID,
                    raceEvent.OrganiserID,
                    raceEvent.EnvironmentID,
                    raceEvent.EventName,
                    raceEvent.EventDescription,
                    raceEvent.EventDate,
                    raceEvent.EventTime,
                    raceEvent.Location,
                    raceEvent.MaxParticipants,
                    raceEvent.EntryFee,
                    raceEvent.Status,
                    raceEvent.CreatedAt
                })
                .ToListAsync();

            return Ok(events);
        }
    }
}
