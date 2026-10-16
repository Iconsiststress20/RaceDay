using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var enrollments = await _context.EventEnrollments
                .Select(enrollment => new
                {
                    enrollment.EnrollmentID,
                    enrollment.ParticipantID,
                    enrollment.EventID,
                    enrollment.CategoryID,
                    enrollment.EnrollmentDate,
                    enrollment.Status,
                    enrollment.PaymentStatus,
                    enrollment.RaceNumber
                })
                .ToListAsync();

            return Ok(enrollments);
        }
    }
}
