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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Cinema(One) - Room(Many) 
        modelBuilder.Entity<Cinema>()
            .HasMany(c => c.Rooms)
            .WithOne(r => r.Cinema)
            .HasForeignKey(r => r.CinemaId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}
