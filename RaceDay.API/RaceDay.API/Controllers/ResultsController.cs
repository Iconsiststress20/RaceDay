using System.Security.Claims;
using RaceDay.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;

namespace RaceDay.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ResultsController(RaceDayContext context)
        {
            _context = context;
        }

        private async Task<int?> GetOrganiserIdAsync()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return null;
            }

            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserID == userId);

            return organiser?.OrganiserID;
        }

        // ORGANISER: View results for their own events
        [Authorize(Roles = "Organiser")]
        [HttpGet]
        public async Task<IActionResult> GetResults()
        {
            var organiserId = await GetOrganiserIdAsync();

            if (organiserId == null)
                return Unauthorized();

            var results = await (
                from result in _context.Results
                join enrollment in _context.EventEnrollments
                    on result.EnrollmentID equals enrollment.EnrollmentID
                join ev in _context.Events
                    on enrollment.EventID equals ev.EventID
                join participant in _context.Participants
                    on enrollment.ParticipantID equals participant.ParticipantID
                join user in _context.Users
                    on participant.UserID equals user.UserID
                where ev.OrganiserID == organiserId
                orderby ev.EventDate descending
                select new
                {
                    result.ResultID,
                    result.EnrollmentID,
                    ev.EventName,
                    ParticipantName = user.FirstName + " " + user.LastName,
                    enrollment.RaceNumber,
                    result.FinishTime,
                    result.OverallPosition,
                    result.CategoryPosition,
                    result.Points,
                    result.Remarks,
                    result.RecordedAt
                }).ToListAsync();

            return Ok(results);
        }

        // PARTICIPANT: View their own results
        [Authorize(Roles = "Participant")]
        [HttpGet("my-results")]
        public async Task<IActionResult> GetMyResults()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
                return Unauthorized();

            var results = await (
                from result in _context.Results
                join enrollment in _context.EventEnrollments
                    on result.EnrollmentID equals enrollment.EnrollmentID
                join ev in _context.Events
                    on enrollment.EventID equals ev.EventID
                where _context.Participants.Any(p =>
                    p.ParticipantID == enrollment.ParticipantID &&
                    p.UserID == userId)
                select new
                {
                    result.ResultID,
                    result.EnrollmentID,
                    ev.EventName,
                    enrollment.RaceNumber,
                    result.FinishTime,
                    result.OverallPosition,
                    result.CategoryPosition,
                    result.Points,
                    result.Remarks,
                    result.RecordedAt
                }).ToListAsync();

            return Ok(results);
        }

        // ORGANISER: Add a result
        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<IActionResult> CreateResult(
            CreateResultRequest request)
        {
            var organiserId = await GetOrganiserIdAsync();

            if (organiserId == null)
                return Unauthorized();

            if (!IsValidResult(
                request.FinishTime,
                request.OverallPosition,
                request.CategoryPosition,
                request.Points))
            {
                return BadRequest(new
                {
                    message = "Check the finish time, positions and points. Positions must be positive and points cannot be negative."
                });
            }

            var enrollment = await (
                from e in _context.EventEnrollments
                join ev in _context.Events
                    on e.EventID equals ev.EventID
                where e.EnrollmentID == request.EnrollmentID
                    && ev.OrganiserID == organiserId
                select e
            ).FirstOrDefaultAsync();

            if (enrollment == null)
            {
                return NotFound(new
                {
                    message = "The enrolment was not found or does not belong to your events."
                });
            }

            var resultExists = await _context.Results
                .AnyAsync(r =>
                    r.EnrollmentID == request.EnrollmentID);

            if (resultExists)
            {
                return Conflict(new
                {
                    message = "A result already exists for this enrolment."
                });
            }

            var result = new Result
            {
                EnrollmentID = request.EnrollmentID,
                FinishTime = request.FinishTime,
                OverallPosition = request.OverallPosition,
                CategoryPosition = request.CategoryPosition,
                Points = request.Points,
                Remarks = request.Remarks?.Trim(),
                RecordedAt = DateTime.UtcNow
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return Created(
                $"/api/Results/{result.ResultID}",
                new
                {
                    result.ResultID,
                    result.EnrollmentID,
                    result.FinishTime,
                    result.OverallPosition,
                    result.CategoryPosition,
                    result.Points,
                    result.Remarks,
                    result.RecordedAt
                });
        }

        // ORGANISER: Update a result
        [Authorize(Roles = "Organiser")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateResult(
            int id,
            UpdateResultRequest request)
        {
            var organiserId = await GetOrganiserIdAsync();

            if (organiserId == null)
                return Unauthorized();

            if (!IsValidResult(
                request.FinishTime,
                request.OverallPosition,
                request.CategoryPosition,
                request.Points))
            {
                return BadRequest(new
                {
                    message = "Check the finish time, positions and points. Positions must be positive and points cannot be negative."
                });
            }

            var result = await (
                from r in _context.Results
                join enrollment in _context.EventEnrollments
                    on r.EnrollmentID equals enrollment.EnrollmentID
                join ev in _context.Events
                    on enrollment.EventID equals ev.EventID
                where r.ResultID == id
                    && ev.OrganiserID == organiserId
                select r
            ).FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound(new
                {
                    message = "The result was not found or does not belong to your events."
                });
            }

            result.FinishTime = request.FinishTime;
            result.OverallPosition = request.OverallPosition;
            result.CategoryPosition = request.CategoryPosition;
            result.Points = request.Points;
            result.Remarks = request.Remarks?.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                result.ResultID,
                result.EnrollmentID,
                result.FinishTime,
                result.OverallPosition,
                result.CategoryPosition,
                result.Points,
                result.Remarks,
                result.RecordedAt
            });
        }

        // ORGANISER: Delete a result
        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var organiserId = await GetOrganiserIdAsync();

            if (organiserId == null)
                return Unauthorized();

            var result = await (
                from r in _context.Results
                join enrollment in _context.EventEnrollments
                    on r.EnrollmentID equals enrollment.EnrollmentID
                join ev in _context.Events
                    on enrollment.EventID equals ev.EventID
                where r.ResultID == id
                    && ev.OrganiserID == organiserId
                select r
            ).FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound(new
                {
                    message = "The result was not found or does not belong to your events."
                });
            }

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool IsValidResult(
            TimeSpan? finishTime,
            int? overallPosition,
            int? categoryPosition,
            int? points)
        {
            return
                (!finishTime.HasValue || finishTime.Value >= TimeSpan.Zero) &&
                (!overallPosition.HasValue || overallPosition.Value > 0) &&
                (!categoryPosition.HasValue || categoryPosition.Value > 0) &&
                (!points.HasValue || points.Value >= 0);
        }
    }

    public class CreateResultRequest
    {
        public int EnrollmentID { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? OverallPosition { get; set; }
        public int? CategoryPosition { get; set; }
        public int? Points { get; set; }
        public string? Remarks { get; set; }
    }

    public class UpdateResultRequest
    {
        public TimeSpan? FinishTime { get; set; }
        public int? OverallPosition { get; set; }
        public int? CategoryPosition { get; set; }
        public int? Points { get; set; }
        public string? Remarks { get; set; }
    }
}