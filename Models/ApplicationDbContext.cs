
using Microsoft.EntityFrameworkCore;

namespace VirtualEventTicketingSystem.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Purchase> Purchases { get; set; }
    public DbSet<EventPurchase> EventPurchases { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EventPurchase>()
            .HasKey(ep => new { ep.EventId, ep.PurchaseId });
        modelBuilder.Entity<Category>()
            .HasMany(c => c.Events)
            .WithOne(e => e.Category)
            .HasForeignKey(e => e.CategoryId);
        
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Webinar", Description = "Online educational sessions" },
            new Category { Id = 2, Name = "Concert", Description = "Live musical performances" },
            new Category { Id = 3, Name = "Workshop", Description = "Interactive training sessions" },
            new Category { Id = 4, Name = "Conference", Description = "Professional Meetings" }
        );
        
        modelBuilder.Entity<Event>().HasData(
            new Event { Id = 1, Title = "C# Fundamentals", CategoryId = 1, DateTime = DateTime.Now.AddDays(5), TicketPrice = 15.99M, AvailableTickets = 10 },
            new Event { Id = 2, Title = "Rock Night Live", CategoryId = 2, DateTime = DateTime.Now.AddDays(10), TicketPrice = 30.00M, AvailableTickets = 3 },
            new Event { Id = 3, Title = "UI/UX Workshop", CategoryId = 3, DateTime = DateTime.Now.AddDays(15), TicketPrice = 25.50M, AvailableTickets = 8 }
        );
        
    }
}