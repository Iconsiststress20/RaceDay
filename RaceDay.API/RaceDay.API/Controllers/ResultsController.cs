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
        [Authorize(Roles = "Organiser")]
        [HttpGet]
        public async Task<IActionResult> GetResults()
        {
            var results = await _context.Results
                .Select(result => new
                {
                    result.ResultID,
                    result.EnrollmentID,
                    result.FinishTime,
                    result.OverallPosition,
                    result.CategoryPosition,
                    result.Points,
                    result.Remarks,
                    result.RecordedAt
                })
                .ToListAsync();

            return Ok(results);
        }

        [Authorize(Roles = "Participant")]
        [HttpGet("my-results")]
        public async Task<IActionResult> GetMyResults()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var results = await _context.Results
                .Where(result => _context.EventEnrollments.Any(enrollment =>
                    enrollment.EnrollmentID == result.EnrollmentID &&
                    _context.Participants.Any(participant =>
                        participant.ParticipantID == enrollment.ParticipantID &&
                        participant.UserID == userId)))
                .Select(result => new
                {
                    result.ResultID,
                    result.EnrollmentID,
                    result.FinishTime,
                    result.OverallPosition,
                    result.CategoryPosition,
                    result.Points,
                    result.Remarks,
                    result.RecordedAt
                })
                .ToListAsync();

            return Ok(results);
        }

        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<IActionResult> CreateResult(
            CreateResultRequest request)
        {
            var enrollmentExists = await _context.EventEnrollments
                .AnyAsync(enrollment =>
                    enrollment.EnrollmentID == request.EnrollmentID);

            if (!enrollmentExists)
            {
                return NotFound(new
                {
                    message = "The event enrolment was not found."
                });
            }

            var resultExists = await _context.Results
                .AnyAsync(result =>
                    result.EnrollmentID == request.EnrollmentID);

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

        [Authorize(Roles = "Organiser")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateResult(
            int id,
            UpdateResultRequest request)
        {
            var result = await _context.Results
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "The result was not found."
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

        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var result = await _context.Results
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "The result was not found."
                });
            }

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }



    }
}