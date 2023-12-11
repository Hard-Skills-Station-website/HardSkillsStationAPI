using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace HardSkillStation.Model
{
    public class LanguageJson
    {
        public int Id;
        public string JsonString;
        public LanguageJson(int id, string jsonString)
        {
            Id = id;
            JsonString = jsonString;
        }
        [System.Text.Json.Serialization.JsonConstructor]
        public LanguageJson() { }
    }
    public class LanguageContext : DbContext
    {
        public LanguageContext(DbContextOptions<LanguageContext> options) : base(options) { }

        public DbSet<LanguageJson> Languages { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define the primary key for the Category entity
            modelBuilder.Entity<LanguageJson>().HasKey(c => c.Id);

            // Other configurations...

            // Call the base implementation to apply any additional configurations
            base.OnModelCreating(modelBuilder);
        }
        // Additional methods for serialization/deserialization
        public LanguageJson DeserializeLanguageJson(string jsonString)
        {
            return JsonConvert.DeserializeObject<LanguageJson>(jsonString);
        }

        public string SerializeLanguageJson(LanguageJson languageJson)
        {
            return JsonConvert.SerializeObject(languageJson);
        }
    }
}
