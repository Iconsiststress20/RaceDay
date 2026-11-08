
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
    public class EventCategoriesController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EventCategoriesController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/EventCategories
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

        // POST: api/EventCategories
        [HttpPost]
        public async Task<IActionResult> CreateCategory(EventCategory request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = new EventCategory
            {
                CategoryName = request.CategoryName,
                Description = request.Description,
                DistanceKM = request.DistanceKM,
                MinAge = request.MinAge,
                MaxAge = request.MaxAge,
                CreatedAt = DateTime.UtcNow
            };

            _context.EventCategories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category created successfully.",
                category.CategoryID
            });
        }

        // PUT: api/EventCategories/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(
            int id, EventCategory request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _context.EventCategories
                .FindAsync(id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            category.CategoryName = request.CategoryName;
            category.Description = request.Description;
            category.DistanceKM = request.DistanceKM;
            category.MinAge = request.MinAge;
            category.MaxAge = request.MaxAge;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category updated successfully."
            });
        }

        // DELETE: api/EventCategories/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.EventCategories
                .FindAsync(id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            _context.EventCategories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category deleted successfully."
            });
        }
    }
}
