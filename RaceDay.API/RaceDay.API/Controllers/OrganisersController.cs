using Microsoft.AspNetCore.Identity;
using RaceDay.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;

namespace RaceDay.Controllers
{
    [Authorize(Roles = "Organiser")]
    [ApiController]
    [Route("api/[controller]")]
    public class OrganisersController : ControllerBase
    {
        private readonly RaceDayContext _context;

        private readonly IPasswordHasher<User> _passwordHasher;

        public OrganisersController(
            RaceDayContext context,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }
        [HttpGet]
        public async Task<IActionResult> GetOrganisers()
        {
            var organisers = await _context.Organisers
                .Select(organiser => new
                {
                    organiser.OrganiserID,
                    organiser.UserID,
                    organiser.OrganisationName,
                    organiser.ContactEmail,
                    organiser.ContactPhone
                })
                .ToListAsync();

            return Ok(organisers);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterOrganiser(
            OrganiserRegisterRequest request)
        {
            var email = request.Email.Trim();

            var emailExists = await _context.Users
                .AnyAsync(user => user.Email == email);

            if (emailExists)
            {
                return Conflict(new
                {
                    message = "An account with this email already exists."
                });
            }

            var user = new User
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                PhoneNumber = request.ContactPhone?.Trim(),
                Role = "Organiser",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.Password);

            var organiser = new Organiser
            {
                UserID = user.UserID,
                OrganisationName = request.OrganisationName.Trim(),
                ContactEmail = request.ContactEmail.Trim(),
                ContactPhone = request.ContactPhone?.Trim()
            };

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            organiser.UserID = user.UserID;
            _context.Organisers.Add(organiser);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Created("/api/Organisers", new
            {
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role,
                organiser.OrganiserID,
                organiser.OrganisationName,
                organiser.ContactEmail,
                organiser.ContactPhone
            });
        }

    }
}
