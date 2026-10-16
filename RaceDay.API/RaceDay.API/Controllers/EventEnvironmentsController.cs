using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;

namespace RaceDay.Controllers
{
    [Authorize(Roles = "Organiser")]
    [ApiController]
    [Route("api/[controller]")]
    public class EventEnvironmentsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EventEnvironmentsController(RaceDayContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEventEnvironments()
        {
            var environments = await _context.EventEnvironments
                .Select(environment => new
                {
                    environment.EnvironmentID,
                    environment.EnvironmentName,
                    environment.EnvironmentType,
                    environment.Description,
                    environment.Address,
                    environment.City,
                    environment.Province,
                    environment.RouteInformation,
                    environment.CreatedAt
                })
                .ToListAsync();

            return Ok(environments);
        }
    }
}
