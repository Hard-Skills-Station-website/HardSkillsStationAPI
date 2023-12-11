using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HardSkillStation.Model
{
    [Table("Categories")]
    public class Category
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public Category(int id, string categoryName)
        {
            Id = id;
            CategoryName = categoryName;
        }
        [JsonConstructor]
        public Category() { }
    }
    public class CategoryContext : DbContext
    {
        public CategoryContext(DbContextOptions<CategoryContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
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
