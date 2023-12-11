using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace HardSkillStation.Model
{
    public class EventTranslation
    {
        public int Id {  get; set; }
        public int? EventId {  get; set; }
        public string Language {  get; set; }
        public string Headline { get; set; }
        public string Summary {  get; set; }
        public string Description { get; set; }

        // Navigation property to the related Event
        [JsonIgnore]
        public Event Event { get; set; }

        public EventTranslation(int? eventId, string language, string headline, string summary, string description)
        {
            EventId = eventId;
            Language = language;
            Headline = headline;
            Summary = summary;
            Description = description;
        }
        [JsonConstructor]
        public EventTranslation() { }
    }
    public class EventTranslationContext : DbContext
    {
        public EventTranslationContext(DbContextOptions<EventTranslationContext> options) : base(options) { }

        public DbSet<EventTranslation> EventTranslations { get; set; }
        public DbSet<Event> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventTranslation>()
                .ToTable("EventTranslation") // Specify the correct table name
                .HasKey(et => et.Id);

            modelBuilder.Entity<EventTranslation>()
                .HasOne(et => et.Event)
                .WithMany(e => e.Translations)
                .HasForeignKey(et => et.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Additional configurations if necessary

            base.OnModelCreating(modelBuilder);
        }
    }
}
