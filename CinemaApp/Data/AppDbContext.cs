using CinemaApp.Model;

using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext() { }
    
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
    {
    }

    public DbSet<Movie> Movies { get; set; }
    public DbSet<Cinema> Cinemas { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Seat> Seats { get; set; }
    public DbSet<Screening> Screenings { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<User> Users { get; set; }

    public DbSet<ProjectionType> ProjectionTypes { get; set; }
    public DbSet<SeatType> SeatTypes { get; set; }
    public DbSet<PersonType> PersonTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Cinema(One) - Room(Many) 
        modelBuilder.Entity<Cinema>()
            .HasMany(c => c.Rooms)
            .WithOne(r => r.Cinema)
            .HasForeignKey(r => r.CinemaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Room(One) - Seat(Many) 
        modelBuilder.Entity<Room>()
            .HasMany(r => r.Seats)
            .WithOne(s => s.Room)
            .HasForeignKey(s => s.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Movie(One) - Screening(Many) 
        modelBuilder.Entity<Movie>()
            .HasMany(m => m.Screenings)
            .WithOne(s => s.Movie)
            .HasForeignKey(s => s.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        // Room(One) - Screening(Many)
        modelBuilder.Entity<Room>()
            .HasMany(r => r.Screenings)
            .WithOne(s => s.Room)
            .HasForeignKey(s => s.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Booking(One) - Ticket(Many)
        modelBuilder.Entity<Booking>()
            .HasMany(b => b.Tickets)
            .WithOne(t => t.Booking)
            .HasForeignKey(t => t.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index to ensure a seat is only used once per screening
        modelBuilder.Entity<Ticket>()
            .HasIndex(t => new { t.ScreeningId, t.SeatId })
            .IsUnique();

        // User(One) - Booking(Many)
        modelBuilder.Entity<User>()
            .HasMany(u => u.Bookings)
            .WithOne(b => b.User)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Add unique constraint to user email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Seed SeatType table
        modelBuilder.Entity<SeatType>().HasData(
            new SeatType { Id = 1, Name = "Regular" },
            new SeatType { Id = 2, Name = "VIP" }
        );

        // Seed ProjectionType table
        modelBuilder.Entity<ProjectionType>().HasData(
            new ProjectionType { Id = 1, Name = "2D" },
            new ProjectionType { Id = 2, Name = "3D" }
        );

        base.OnModelCreating(modelBuilder);
    }
}
