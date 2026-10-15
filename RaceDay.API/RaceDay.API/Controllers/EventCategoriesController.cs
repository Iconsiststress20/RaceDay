using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventCategoriesController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EventCategoriesController(RaceDayContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEventCategories()
        {
            var categories = await _context.EventCategories
                .Select(category => new
                {
                    category.CategoryID,
                    category.CategoryName,
                    category.Description,
                    category.DistanceKM,
                    category.MinAge,
                    category.MaxAge,
                    category.CreatedAt
                })
                .ToListAsync();

            return Ok(categories);
        }
    }
}