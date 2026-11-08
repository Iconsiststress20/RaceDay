using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

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

        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventRequest request)
        {
            var userId = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
            );

            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserID == userId);

            if (organiser == null)
            {
                return BadRequest(new
                {
                    message = "Organiser profile not found."
                });
            }

            var environmentExists = await _context.EventEnvironments
                .AnyAsync(e => e.EnvironmentID == request.EnvironmentID);

            if (!environmentExists)
            {
                return BadRequest(new
                {
                    message = "Invalid event environment."
                });
            }

            var newEvent = new RaceDay.Models.Event
            {
                OrganiserID = organiser.OrganiserID,
                EnvironmentID = request.EnvironmentID,
                EventName = request.EventName,
                EventDescription = request.EventDescription,
                EventDate = request.EventDate,
                EventTime = request.EventTime,
                Location = request.Location,
                MaxParticipants = request.MaxParticipants,
                EntryFee = request.EntryFee,
                Status = request.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(newEvent);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvents),
                new { id = newEvent.EventID },
                new
                {
                    message = "Event created successfully.",
                    newEvent.EventID,
                    newEvent.EventName
                }
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(
    int id, UpdateEventRequest request)
        {
            var userId = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
            );

            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserID == userId);

            if (organiser == null)
            {
                return BadRequest(new
                {
                    message = "Organiser profile not found."
                });
            }

            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e =>
                    e.EventID == id &&
                    e.OrganiserID == organiser.OrganiserID);

            if (raceEvent == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            var environmentExists = await _context.EventEnvironments
                .AnyAsync(e => e.EnvironmentID == request.EnvironmentID);

            if (!environmentExists)
            {
                return BadRequest(new
                {
                    message = "Invalid event environment."
                });
            }

            raceEvent.EventName = request.EventName;
            raceEvent.EventDescription = request.EventDescription;
            raceEvent.EnvironmentID = request.EnvironmentID;
            raceEvent.EventDate = request.EventDate;
            raceEvent.EventTime = request.EventTime;
            raceEvent.Location = request.Location;
            raceEvent.MaxParticipants = request.MaxParticipants;
            raceEvent.EntryFee = request.EntryFee;
            raceEvent.Status = request.Status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event updated successfully."
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
            );

            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserID == userId);

            if (organiser == null)
            {
                return BadRequest(new
                {
                    message = "Organiser profile not found."
                });
            }

            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e =>
                    e.EventID == id &&
                    e.OrganiserID == organiser.OrganiserID);

            if (raceEvent == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            _context.Events.Remove(raceEvent);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event deleted successfully."
            });
        }
    }
}
