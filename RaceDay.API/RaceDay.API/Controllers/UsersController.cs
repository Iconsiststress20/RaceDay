using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using Microsoft.AspNetCore.Identity;
using RaceDay.Models;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;

        public UsersController(
    RaceDayContext context,
    IPasswordHasher<User> passwordHasher,
    IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(user => new
                {
                    user.UserID,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.PhoneNumber,
                    user.Role,
                    user.IsActive,
                    user.CreatedAt
                })
                .ToListAsync();

            return Ok(users);


        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
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
                PhoneNumber = request.PhoneNumber?.Trim(),
                Role = "Participant",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Created($"/api/Users/{user.UserID}", new
            {
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.Role,
                user.IsActive,
                user.CreatedAt
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var email = request.Email.Trim();

            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Email == email);

            if (user == null || !user.IsActive)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            var passwordResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }


            var jwtKey = _configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                return StatusCode(500, new
                {
                    message = "Authentication configuration is missing."
                });
            }

            var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
    new Claim(ClaimTypes.Name, user.Email),
    new Claim(ClaimTypes.Role, user.Role)
};

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "RaceDay",
                audience: "RaceDayApi",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler()
                .WriteToken(token);

            return Ok(new
            {
                message = "Login successful.",
                token = tokenString,
                expiresAt = token.ValidTo,
                user = new
                {
                    user.UserID,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.Role
                }
            });

        }

        [Authorize]
        [HttpGet("protected-test")]
        public IActionResult ProtectedTest()
        {
            return Ok(new
            {
                message = "You are authenticated!",
                userId = User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                email = User.Identity?.Name
            });
        }



    }
}
