
using Microsoft.EntityFrameworkCore;
using RaceDay.Models;

namespace RaceDay.Data
{
    public class RaceDayContext : DbContext
    {
        public RaceDayContext(DbContextOptions<RaceDayContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Participant> Participants { get; set; }

        public DbSet<Organiser> Organisers { get; set; }
        public DbSet<EventCategory> EventCategories { get; set; }
        public DbSet<EventEnvironment> EventEnvironments { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventEnrollment> EventEnrollments { get; set; }
        public DbSet<Result> Results { get; set; }
    }
}
    