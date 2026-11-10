
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using RaceDay.Data;

namespace RaceDay.Controllers
{
    [Authorize(Roles = "Participant")]
    [ApiController]
    [Route("api/[controller]")]
    public class ParticipantEnrollmentsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ParticipantEnrollmentsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/ParticipantEnrollments
        [HttpGet]
        public async Task<IActionResult> GetMyEnrollments()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserID == userId);

            if (participant == null)
                return NotFound(new
                {
                    message = "Participant profile not found."
                });

            var enrollments =
                await (from enrollment in _context.EventEnrollments
                       join ev in _context.Events
                           on enrollment.EventID equals ev.EventID
                       join category in _context.EventCategories
                           on enrollment.CategoryID equals category.CategoryID
                       where enrollment.ParticipantID == participant.ParticipantID
                       orderby enrollment.EnrollmentDate descending
                       select new
                       {
                           enrollment.EnrollmentID,
                           enrollment.EventID,
                           ev.EventName,
                           ev.EventDate,
                           ev.EventTime,
                           ev.Location,
                           enrollment.CategoryID,
                           category.CategoryName,
                           category.DistanceKM,
                           enrollment.EnrollmentDate,
                           enrollment.Status,
                           enrollment.PaymentStatus,
                           enrollment.RaceNumber
                       }).ToListAsync();

            return Ok(enrollments);
        }

        // POST: api/ParticipantEnrollments
        [HttpPost]
        public async Task<IActionResult> Enrol(
            CreateParticipantEnrollmentRequest request)
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserID == userId);

            if (participant == null)
                return NotFound(new
                {
                    message = "Participant profile not found."
                });

            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == request.EventID);

            if (raceEvent == null)
                return NotFound(new
                {
                    message = "Event not found."
                });

            if (raceEvent.EventDate.Date <= DateTime.Today)
                return BadRequest(new
                {
                    message = "This event is no longer accepting enrolments."
                });

            var categoryExists = await _context.EventCategories
                .AnyAsync(c => c.CategoryID == request.CategoryID);

            if (!categoryExists)
                return BadRequest(new
                {
                    message = "Please select a valid category."
                });

            var alreadyEnrolled = await _context.EventEnrollments
                .AnyAsync(e =>
                    e.ParticipantID == participant.ParticipantID &&
                    e.EventID == request.EventID);

            if (alreadyEnrolled)
                return Conflict(new
                {
                    message = "You are already enrolled in this event."
                });

            var currentEnrolments = await _context.EventEnrollments
                .CountAsync(e => e.EventID == request.EventID);

            if (currentEnrolments >= raceEvent.MaxParticipants)
                return BadRequest(new
                {
                    message = "This event is full."
                });

            var enrollment = new RaceDay.Models.EventEnrollment
            {
                ParticipantID = participant.ParticipantID,
                EventID = request.EventID,
                CategoryID = request.CategoryID,
                EnrollmentDate = DateTime.UtcNow,
                Status = "Registered",
                PaymentStatus = "Pending",
                RaceNumber = null
            };

            _context.EventEnrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "You have successfully enrolled in this event.",
                enrollment.EnrollmentID
            });
        }
    }

    public class CreateParticipantEnrollmentRequest
    {
        public int EventID { get; set; }
        public int CategoryID { get; set; }
    }
}
