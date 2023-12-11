using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HardSkillStation.Model
{
    [Table("UserEvent")]
    public class UserEvent
    {
        public int Id { get; set; } // Primary key

        public int UserId { get; set; } // Foreign key - User
        //public User User { get; set; } // Navigation property

        public int EventId { get; set; } // Foreign key - Events
        //public Event Event { get; set; } // Navigation property
        public UserEvent(int userId, int eventId)
        {
            UserId = userId;
            EventId = eventId;
        }
        [JsonConstructor]
        public UserEvent() { }
    }

    public class UserEventContext : DbContext
    {
        public UserEventContext(DbContextOptions<UserEventContext> options) : base(options) { }

        public DbSet<Event> Events { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserEvent> UserEvents { get; set; } // Add the junction table to your DbContext
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define the primary key for the Category entity
            modelBuilder.Entity<Category>().HasKey(c => c.Id);

            // Other configurations...

            // Call the base implementation to apply any additional configurations
            base.OnModelCreating(modelBuilder);
        }
    }


}
