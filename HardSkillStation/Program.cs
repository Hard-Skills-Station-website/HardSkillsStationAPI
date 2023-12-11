using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using HardSkillStation.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Cors;
using HardSkillStation.Logic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace HardSkillStation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Contexts
            builder.Services.AddDbContext<EventContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDbContext<UserContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDbContext<UserEventContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDbContext<CategoryContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDbContext<LanguageContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDbContext<EventTranslationContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add services to the container.
            builder.Services.AddRazorPages();

            var app = builder.Build();

            app.UseSwaggerUI();

            // API Functions
            app.MapGet("/", () => "Hard Skill Station API");
            // Get User (indeholder users password, så den er meget usikker og burde fjernes ved launch)
            app.MapGet("/User", (UserContext dbContext) =>
            {
                try
                {
                    var allUsers = dbContext.Users.ToList();
                    return Results.Ok(allUsers);
                }
                catch (Exception ex)
                {
                    // Log af fejlmeddelelse
                    Console.WriteLine($"An error occurred: {ex.Message}");

                    // Response til brugerclient
                    return Results.BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
                }
            });
            // Create User
            app.MapPost("/createUser", async (User user, UserContext dbContext) =>
            {
                try
                {
                    dbContext.Users.Add(user);
                    await dbContext.SaveChangesAsync();
                    return Results.Created($"/users/{user.Id}", user); // Returner lavet bruger
                }
                catch (Exception ex)
                {
                    // Log af fejlmeddelelse
                    Console.WriteLine($"An error occurred: {ex.Message}");

                    // Response til brugerclient
                    return Results.BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
                }
            });

            app.MapPost("/login", async (LoginRequest loginRequest, UserContext dbContext) =>
            {
                try
                {
                    // Søger efter email
                    var user = dbContext.Users.FirstOrDefault(u => u.Email == loginRequest.Email);

                    if (user == null)
                    {
                        return Results.NotFound("User not found");
                    }

                    // Verificere password
                    if (VerifyPassword(loginRequest.Password, user.Password))
                    {
                        Console.WriteLine($"Login successful for user with email: {user.Email}");
                        // Password er korrekt, insensitiv brugerdata sendes som respons
                        var userResponse = new
                        {
                            user.Id,
                            user.FirstName,
                            user.LastName,
                            user.Email,
                            // ... mulighed for flere felter
                        };

                        return Results.Ok(userResponse);
                    }
                    else
                    {
                        // Forkert password
                        return Results.BadRequest("Incorrect password");
                    }
                }
                catch (Exception ex)
                {
                    // Log af fejl
                    Console.WriteLine($"An error occurred: {ex.Message}");

                    // Response til klient
                    return Results.BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
                }
            });
            //// Fetch and Return Serialized JSON
            //app.MapGet("/getSerializedJson", async (LanguageContext dbContext) =>
            //{
            //    try
            //    {
            //        // Assume you have a LanguageJson entity with an Id
            //        var languageJson = await dbContext.Languages.FindAsync(1);

            //        if (languageJson == null)
            //        {
            //            return Results.NotFound("LanguageJson not found");
            //        }

            //        // Deserialize the stored JSON string
            //        var deserializedJson = JsonSerializer.Deserialize<LanguageJson>(languageJson.JsonString);

            //        // Serialize the JSON to send to the frontend
            //        var serializedJson = JsonSerializer.Serialize(deserializedJson);

            //        return Results.Ok(serializedJson);
            //    }
            //    catch (Exception ex)
            //    {
            //        // Log the error
            //        Console.WriteLine($"An error occurred: {ex.Message}");

            //        // Response to client
            //        return Results.BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
            //    }
            //});
            // Lav UserEvent table registrering
            app.MapPost("/registerUserForEvent", async (UserEvent userEvent, UserEventContext dbContext) =>
            {
                try
                {
                    // UserEvent skal have UserId og EventId
                    dbContext.UserEvents.Add(userEvent);
                    await dbContext.SaveChangesAsync();
                    return Results.Created($"/registrations/{userEvent.Id}", userEvent);
                }
                catch (Exception ex)
                {
                    // Log og error besked
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    return Results.BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
                }
            });
            // Get Categories
            app.MapGet("/Category", (CategoryContext dbContext) =>
            {
                try
                {
                    var allCategories = dbContext.Categories.ToList();
                    return Results.Ok(allCategories);
                }
                catch (Exception ex)
                {
                    // Log af fejlmeddelelse
                    Console.WriteLine($"An error occurred: {ex.Message}");

                    // Response til brugerclient
                    return Results.BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
                }
            });
            // Create Category
            app.MapPost("/createCategory", async (Category category, CategoryContext dbContext) =>
            {
                try
                {
                    dbContext.Categories.Add(category);
                    await dbContext.SaveChangesAsync();
                    return Results.Created($"/categories/{category.Id}", category); // Returner lavet Category
                }
                catch (Exception ex)
                {
                    // Log af fejlmeddelelse
                    Console.WriteLine($"An error occurred: {ex.Message}");

                    // Response til brugerclient
                    return Results.BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
                }
            });
            app.MapPut("/editCategory/{categoryId}", async (int categoryId, Category updateCategory, CategoryContext dbContext) =>
            {
                try
                {
                    // Find the existing category in the database
                    var existingCategory = await dbContext.Categories.FindAsync(categoryId);

                    if (existingCategory == null)
                    {
                        return Results.NotFound($"No category with ID {categoryId} found");
                    }

                    // Opdater kategori data (lige nu kun navn)
                    existingCategory.CategoryName = updateCategory.CategoryName;

                    // Gem ændringer i database
                    await dbContext.SaveChangesAsync();

                    // Return succesfuld respons
                    return Results.Ok(existingCategory);
                }
                catch (Exception ex)
                {
                    // Log af fejl
                    Console.WriteLine($"An error occurred: {ex.Message}");

                    // Returner en fejlbesked
                    return Results.BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
                }
            });
            // Delete Category
            app.MapDelete("/deleteCategory/{categoryId}", async (int categoryId, CategoryContext dbContext) =>
            {
                try
                {
                    var category = await dbContext.Categories.FindAsync(categoryId);

                    if (category == null)
                    {
                        return Results.NotFound($"No category with ID {categoryId} found");
                    }

                    dbContext.Categories.Remove(category); // Fjern Category fra dbContext
                    await dbContext.SaveChangesAsync();

                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    // Log af fejlmeddelelse
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    // Response til brugerclient
                    return Results.BadRequest("Error deleting category");
                }
            });
            // Get event by language and ?status
            app.MapGet("/events/{language}", (EventContext dbContext, string language, string? status) =>
            {
                try
                {
                    var currentDate = DateTime.Now;

                    // Load events med translations
                    var eventsWithTranslations = dbContext.Events
                        .Include(e => e.Translations)
                        .Where(e =>
                            (status == null || status.ToLower() == "future") && e.Date > currentDate ||
                            (status != null && status.ToLower() == "past" && e.Date < currentDate) ||
                            (status != null && status.ToLower() == "all"))
                        .ToList();

                    // Erstat sprogsbaserede data med indstillet sprog
                    var filteredEvents = eventsWithTranslations
                        .Select(e => new
                        {
                            e.Id,
                            Headline = e.Translations.FirstOrDefault(et => et.Language.ToLower() == language.ToLower())?.Headline,
                            Summary = e.Translations.FirstOrDefault(et => et.Language.ToLower() == language.ToLower())?.Summary,
                            Description = e.Translations.FirstOrDefault(et => et.Language.ToLower() == language.ToLower())?.Description,
                            e.Image,
                            e.CategoryId,
                            e.Company,
                            e.Location,
                            e.Contact,
                            e.Date,
                            e.IsPublished,
                        })
                        .ToList();

                    return Results.Ok(filteredEvents);
                }
                catch (Exception ex)
                {
                    // Log the error message
                    Console.WriteLine($"An error occurred: {ex.Message}");

                    // Return a bad request response
                    return Results.BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
                }
            });

            //// Create event
            //app.MapPost("/createEvent", async (Event events, EventContext dbContext) =>
            //{
            //    try
            //    {
            //        dbContext.Events.Add(events);
            //        await dbContext.SaveChangesAsync();
            //        return Results.Created($"/categories/{events.Id}", events); // Returner lavet Event
            //    }
            //    catch (Exception ex)
            //    {
            //        // Log af fejlmeddelelse
            //        Console.WriteLine($"An error occurred: {ex.Message}");

            //        // Response til brugerclient
            //        return Results.BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
            //    }
            //});

            // Create event and translation
            app.MapPost("/createEventAndTranslations", async (EventCreationModel model, EventContext eventContext, EventTranslationContext translationContext) =>
            {
                try
                {
                    // Step 1: Lav og gem event
                    var newEvent = new Event
                    {
                        Image = model.EventImage,
                        CategoryId = model.EventCategoryId,
                        Date = model.EventDate,
                        Location = model.EventLocation,
                        Company = model.EventCompany,
                        Contact = model.EventContact,
                        IsPublished = model.EventIsPublished
                    };

                    eventContext.Events.Add(newEvent);
                    await eventContext.SaveChangesAsync();

                    // Step 2: Brug det nye EventId til at lave translations
                    var translations = new List<EventTranslation>();

                    // Danish Translation
                    var danishTranslation = new EventTranslation
                    {
                        EventId = newEvent.Id,
                        Language = "Danish",
                        Headline = model.TranslationHeadlineDanish,
                        Summary = model.TranslationSummaryDanish,
                        Description = model.TranslationDescriptionDanish
                    };
                    translations.Add(danishTranslation);

                    // English translation
                    var englishTranslation = new EventTranslation
                    {
                        EventId = newEvent.Id,
                        Language = "English",
                        Headline = model.TranslationHeadlineEnglish,
                        Summary = model.TranslationSummaryEnglish,
                        Description = model.TranslationDescriptionEnglish
                    };
                    translations.Add(englishTranslation);

                    // Tilføj translations til context
                    translationContext.EventTranslations.AddRange(translations);
                    await translationContext.SaveChangesAsync();

                    // Respons i form af Event og dens translations
                    var createdEventAndTranslations = new
                    {
                        Event = newEvent,
                        Translations = translations
                    };

                    return Results.Created($"/api/events/{newEvent.Id}", createdEventAndTranslations);
                }
                catch (Exception ex)
                {
                    // Log af fejl
                    Console.WriteLine($"An error occurred: {ex.Message}");

                    // Fejl respons
                    return Results.BadRequest("Error creating event and translations");
                }
            });

            // Edit Event
            app.MapPut("/editEvent/{eventId}", async (int eventId, Event updatedEvent, EventContext dbContext) =>
            {
                try
                {
                    // Find event i databasen
                    var existingEvent = await dbContext.Events.FindAsync(eventId);

                    if (existingEvent == null)
                    {
                        return Results.NotFound($"No event with ID {eventId} found");
                    }

                    // Update data med nyt data (hvis tomt så er nyt data tomt)
                    existingEvent.Image = updatedEvent.Image;
                    existingEvent.CategoryId = updatedEvent.CategoryId;
                    existingEvent.Date = updatedEvent.Date;
                    existingEvent.Location = updatedEvent.Location;
                    existingEvent.Company = updatedEvent.Company;
                    existingEvent.Contact = updatedEvent.Contact;
                    existingEvent.IsPublished = updatedEvent.IsPublished;

                    // Gem til database
                    await dbContext.SaveChangesAsync();

                    // returner respons
                    return Results.Ok(existingEvent);
                }
                catch (Exception ex)
                {
                    // Log af fejl
                    Console.WriteLine($"An error occurred: {ex.Message}");

                    // Returner fejl respons
                    return Results.BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
                }
            });
            // Publish Event
            app.MapPut("/publishEvent/{eventId}", async (int eventId, EventContext dbContext) =>
            {
                try
                {
                    // Find den eksisterende event i databasen
                    var existingEvent = await dbContext.Events.FindAsync(eventId);

                    if (existingEvent == null)
                    {
                        return Results.NotFound($"No event with ID {eventId} found");
                    }

                    // opdatering af IsPublished
                    existingEvent.IsPublished = true;

                    // Gem til databasen
                    await dbContext.SaveChangesAsync();

                    // Returner succesfuld respons
                    return Results.Ok(existingEvent);
                }
                catch (Exception ex)
                {
                    // Log af fejl
                    Console.WriteLine($"An error occurred: {ex.Message}");

                    // Returner fejl respons
                    return Results.BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
                }
            });
            // Delete event
            //app.MapDelete("/deleteEvent/{eventId}", async (int eventId, EventContext dbContext) =>
            //{
            //    try
            //    {
            //        var eventToDelete = dbContext.Events
            //            .Include(e => e.Translations)
            //            .FirstOrDefault(e => e.Id == eventId);

            //        if (eventToDelete != null)
            //        {
            //            // Manually delete related translations
            //            dbContext.EventTranslations.RemoveRange(eventToDelete.Translations);

            //            // Now, delete the Event
            //            dbContext.Events.Remove(eventToDelete);

            //            await dbContext.SaveChangesAsync();
            //            return Results.Ok("Event and related translations deleted successfully");
            //        }
            //        else
            //        {
            //            return Results.NotFound("Event not found");
            //        }
            //    }
            //    catch (DbUpdateException ex)
            //    {
            //        // Handle specific DbUpdateException if needed
            //        Console.WriteLine($"DbUpdateException: {ex.Message}");
            //        return Results.BadRequest(new { ErrorMessage = "An error occurred while deleting the event", ExceptionMessage = ex.Message });
            //    }
            //    catch (Exception ex)
            //    {
            //        // Log the error message
            //        Console.WriteLine($"An error occurred: {ex.Message}");
            //        // Return a bad request response
            //        return Results.BadRequest(new { ErrorMessage = "An error occurred", ExceptionMessage = ex.Message });
            //    }
            //});

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.UseSwagger(x => x.SerializeAsV2 = true);

            app.Run();

            bool VerifyPassword(string inputPassword, string storedPassword)
            {
                // Sammenligner passwords direkte (er ikke sikkert, men er en midlertidlig løsning)
                return inputPassword == storedPassword;
            }
        }

    }
}