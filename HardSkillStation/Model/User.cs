using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HardSkillStation.Model
{
    [Table("Users")]
    /*
     * id - int
     * fornavn - string
     * efternavn - string
     * password - string
     * email - string
     * mobil - int
     * company - string
     * Student - Bool
     * Admin - Bool
     */
    public class User
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Company { get; set; }
        public bool IsStudent { get; set; } = false;
        public bool IsAdmin { get; set; } = false;
        //public ICollection<UserEvent> UserEvents { get; set; } // Navigation property
        public User(string? firstName, string? lastName, string email, string password, string? phoneNumber, string? company, bool isStudent, bool isAdmin)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Password = password;
            PhoneNumber = phoneNumber;
            Company = company;
            IsStudent = isStudent;
            IsAdmin = isAdmin;
        }
        [JsonConstructor]
        public User() { }
    }
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
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
