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
    public DbSet<Genre> Genres { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Cinema(One) - Room(Many) 
        modelBuilder.Entity<Cinema>()
            .HasMany(c => c.Rooms)
            .WithOne(r => r.Cinema)
            .HasForeignKey(r => r.CinemaId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Rooms_Cinemas");

        // Room(One) - Seat(Many) 
        modelBuilder.Entity<Room>()
            .HasMany(r => r.Seats)
            .WithOne(s => s.Room)
            .HasForeignKey(s => s.RoomId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Seats_Rooms");

        // Movie(One) - Screening(Many) 
        modelBuilder.Entity<Movie>()
            .HasMany(m => m.Screenings)
            .WithOne(s => s.Movie)
            .HasForeignKey(s => s.MovieId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Screenings_Movies");

        // Movie(Many) - Genre(Many)
        modelBuilder.Entity<Movie>()
            .HasMany(m => m.Genres)
            .WithMany(g => g.Movies)
            .UsingEntity(j => j.ToTable("MovieGenres"));

        // Room(One) - Screening(Many)
        modelBuilder.Entity<Room>()
            .HasMany(r => r.Screenings)
            .WithOne(s => s.Room)
            .HasForeignKey(s => s.RoomId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Screenings_Rooms");

        // Booking(One) - Ticket(Many)
        modelBuilder.Entity<Booking>()
            .HasMany(b => b.Tickets)
            .WithOne(t => t.Booking)
            .HasForeignKey(t => t.BookingId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Tickets_Bookings");

        // Ticket(One) - Seat(One)
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Seat)
            .WithOne()
            .HasForeignKey<Ticket>(t => t.SeatId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Tickets_Seat");

        // Ticket(One) - PersonType(One)
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.PersonType)
            .WithOne()
            .HasForeignKey<Ticket>(t => t.PersonTypeId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Tickets_PersonType");

        // User(One) - Booking(Many)
        modelBuilder.Entity<User>()
            .HasMany(u => u.Bookings)
            .WithOne(b => b.User)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Bookings_Users");

        // Screening(One) - Booking(Many)
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Screening)
            .WithMany()
            .HasForeignKey(b => b.ScreeningId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Bookings_Screenings");

        // Specify precision to price
        modelBuilder.Entity<Movie>()
            .Property(s => s.BasePrice).HasPrecision(5, 2);
        modelBuilder.Entity<Screening>()
            .Property(s => s.BasePrice).HasPrecision(5, 2);
        modelBuilder.Entity<Ticket>()
            .Property(s => s.TotalPrice).HasPrecision(5, 2);
        
        modelBuilder.Entity<User>(entity =>
        {
            // User(One) - Booking(Many)
            entity
                .HasMany(u => u.Bookings)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add unique constraint to user email
            entity
                .HasIndex(u => u.Email)
                .IsUnique();
        });
        
        // Seed SeatType table
        modelBuilder.Entity<SeatType>(entity =>
        {
            entity.Property(s => s.PriceAmountDiscount)
                .HasPrecision(5,2)
                .HasDefaultValue(0m);

            entity.HasData(
                new SeatType { Id = 1, Name = "Regular", PriceAmountDiscount = 0 },
                new SeatType { Id = 2, Name = "VIP", PriceAmountDiscount = 10 }
            );
        });

        // Seed ProjectionType table
        modelBuilder.Entity<ProjectionType>(entity =>
        {
            entity.Property(p => p.PriceAmountDiscount)
                .HasPrecision(5,2)
                .HasDefaultValue(0m);

            entity.HasData(
                new ProjectionType { Id = 1, Name = "2D", PriceAmountDiscount = 0 },
                new ProjectionType { Id = 2, Name = "3D", PriceAmountDiscount = 10 }
            );
        });

        // Seed PersonType table
        modelBuilder.Entity<PersonType>(entity =>
        {
            entity.Property(p => p.PricePercentDiscount)
                .HasPrecision(5,2)
                .HasDefaultValue(0m);
                
            entity.HasData(
                new PersonType { Id = 1, Name = "Adult", PricePercentDiscount = 0 },
                new PersonType { Id = 2, Name = "Child", PricePercentDiscount = 30 },
                new PersonType { Id = 3, Name = "Student", PricePercentDiscount = 20 }
            );
        });

        // Seed Genre table
        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasData(
                new Genre { Id = 1, Name = "Action" },
                new Genre { Id = 2, Name = "Comedy" },
                new Genre { Id = 3, Name = "Drama" },
                new Genre { Id = 4, Name = "Horror" },
                new Genre { Id = 5, Name = "Science Fiction" },
                new Genre { Id = 6, Name = "Thriller" },
                new Genre { Id = 7, Name = "Romance" },
                new Genre { Id = 8, Name = "Adventure" },
                new Genre { Id = 9, Name = "Animation" },
                new Genre { Id = 10, Name = "Documentary" }
            );
        });

        base.OnModelCreating(modelBuilder);
    }
}
