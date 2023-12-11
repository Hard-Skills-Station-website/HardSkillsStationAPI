using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text.Json.Serialization;
using HardSkillStation.Model;

namespace HardSkillStation.Model
{
    [Table("Event")]
    public class Event
    {
        public int Id {get; set;}
        public string? Image { get; set;}
        // Foreign key property for the Category class
        public int? CategoryId { get; set; }

        // Navigation property to the Category class
        [JsonIgnore]
        public Category Category { get; set; }
        public DateTime Date {  get; set; }
        public string Location { get; set; }
        public string? Company { get; set; }
        public string? Contact { get; set; }
        public bool IsPublished { get; set; } = false;
        [JsonIgnore]
        public ICollection<EventTranslation> Translations { get; set; } = new List<EventTranslation>();
        //public ICollection<UserEvent> UserEvents { get; set; } // Navigation property
        public Event(string? image, int? categoryId, DateTime date, string location, string? company, string? contact, bool isPublished)
        {
            Image = image;
            CategoryId = categoryId;
            Date = date;
            Location = location;
            Company = company;
            Contact = contact;
            IsPublished = isPublished;
        }
        [JsonConstructor]
        public Event() { }
    }
    public class EventContext : DbContext
    {
        public EventContext(DbContextOptions<EventContext> options) : base(options) { }

        public DbSet<Event> Events { get; set; }
        public DbSet<EventTranslation> EventTranslation { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define the primary key for the Category entity
            modelBuilder.Entity<Category>().HasKey(c => c.Id);

            modelBuilder.Entity<Event>()
                .HasMany(e => e.Translations)
                .WithOne(t => t.Event)
                .OnDelete(DeleteBehavior.Cascade);

            // Other configurations...

            // Call the base implementation to apply any additional configurations
            base.OnModelCreating(modelBuilder);
        }
    }
}
