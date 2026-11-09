
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using RaceDay.Data;

namespace RaceDay.Controllers
{
    [Authorize(Roles = "Organiser")]
    [ApiController]
    [Route("api/[controller]")]
    public class EventEnrollmentsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EventEnrollmentsController(RaceDayContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEventEnrollments()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized();
            }

            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserID == userId);

            if (organiser == null)
            {
                return NotFound(new
                {
                    message = "Organiser profile not found."
                });
            }

            var enrollments =
                await (from enrollment in _context.EventEnrollments

                       join participant in _context.Participants
                           on enrollment.ParticipantID
                           equals participant.ParticipantID

                       join user in _context.Users
                           on participant.UserID equals user.UserID

                       join ev in _context.Events
                           on enrollment.EventID equals ev.EventID

                       join category in _context.EventCategories
                           on enrollment.CategoryID equals category.CategoryID

                       where ev.OrganiserID == organiser.OrganiserID

                       orderby enrollment.EnrollmentDate descending

                       select new
                       {
                           enrollment.EnrollmentID,

                           ParticipantName =
                               user.FirstName + " " + user.LastName,

                           user.Email,
                           user.PhoneNumber,

                           enrollment.ParticipantID,

                           enrollment.EventID,
                           ev.EventName,
                           ev.EventDate,

                           enrollment.CategoryID,
                           category.CategoryName,
                           category.DistanceKM,

                           enrollment.EnrollmentDate,
                           enrollment.Status,
                           enrollment.PaymentStatus,
                           enrollment.RaceNumber
                       })
                       .ToListAsync();

            return Ok(enrollments);
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEventEnrollment(
            int id,
            UpdateEventEnrollmentRequest request)
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized();
            }

            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserID == userId);

            if (organiser == null)
            {
                return NotFound(new
                {
                    message = "Organiser profile not found."
                });
            }

            var enrollment = await _context.EventEnrollments
                .FirstOrDefaultAsync(e => e.EnrollmentID == id);

            if (enrollment == null)
            {
                return NotFound(new
                {
                    message = "Enrolment not found."
                });
            }

            var ev = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == enrollment.EventID);

            if (ev == null || ev.OrganiserID != organiser.OrganiserID)
            {
                return Forbid();
            }

            var validStatuses = new[]
            {
        "Registered", "Approved", "Rejected", "Cancelled"
    };

            var validPaymentStatuses = new[]
            {
        "Pending", "Paid", "Unpaid"
    };

            var status = validStatuses.FirstOrDefault(
                s => s.Equals(request.Status?.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            var paymentStatus = validPaymentStatuses.FirstOrDefault(
                s => s.Equals(request.PaymentStatus?.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            if (status == null || paymentStatus == null)
            {
                return BadRequest(new
                {
                    message = "Invalid enrolment or payment status."
                });
            }

            if (request.RaceNumber.HasValue && request.RaceNumber.Value <= 0)
            {
                return BadRequest(new
                {
                    message = "Race number must be greater than zero."
                });
            }

            if (request.RaceNumber.HasValue)
            {
                var raceNumberExists = await _context.EventEnrollments
                    .AnyAsync(e =>
                        e.EventID == enrollment.EventID &&
                        e.EnrollmentID != enrollment.EnrollmentID &&
                        e.RaceNumber == request.RaceNumber);

                if (raceNumberExists)
                {
                    return Conflict(new
                    {
                        message = "This race number is already assigned for this event."
                    });
                }
            }

            enrollment.Status = status;
            enrollment.PaymentStatus = paymentStatus;
            enrollment.RaceNumber = request.RaceNumber;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Enrolment updated successfully."
            });
        }

    }
}


public class UpdateEventEnrollmentRequest
{
    public string Status { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;

    public int? RaceNumber { get; set; }
}
