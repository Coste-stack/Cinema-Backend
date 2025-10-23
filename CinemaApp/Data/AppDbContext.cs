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

    public DbSet<ProjectionType> ProjectionTypes { get; set; }

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


        base.OnModelCreating(modelBuilder);
    }
}
