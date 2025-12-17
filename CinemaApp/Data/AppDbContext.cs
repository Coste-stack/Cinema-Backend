using CinemaApp.Model;

using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
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
    public DbSet<Offer> Offers { get; set; }
    public DbSet<OfferCondition> OfferConditions { get; set; }
    public DbSet<OfferEffect> OfferEffects { get; set; }
    public DbSet<AppliedOffer> AppliedOffers { get; set; }

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

        // Screening(Many) - ProjectionType(One)
        modelBuilder.Entity<Screening>()
            .HasOne(s => s.ProjectionType)
            .WithMany()
            .HasForeignKey(s => s.ProjectionTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Screenings_ProjectionTypes");

        // Seat(Many) - SeatType(One)
        modelBuilder.Entity<Seat>()
            .HasOne(s => s.SeatType)
            .WithMany()
            .HasForeignKey(s => s.SeatTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Seats_SeatTypes");

        // Booking(One) - Ticket(Many)
        modelBuilder.Entity<Booking>()
            .HasMany(b => b.Tickets)
            .WithOne(t => t.Booking)
            .HasForeignKey(t => t.BookingId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Tickets_Bookings");

        // Ticket(Many) - Seat(One)
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Seat)
            .WithMany()
            .HasForeignKey(t => t.SeatId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Tickets_Seat");

        // Ticket(Many) - PersonType(One)
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.PersonType)
            .WithMany()
            .HasForeignKey(t => t.PersonTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Tickets_PersonType");

        modelBuilder.Entity<User>(entity =>
        {
            // User(One) - Booking(Many)
            entity
                .HasMany(u => u.Bookings)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Bookings_Users");

            // Add unique constraint to user email
            entity
                .HasIndex(u => u.Email)
                .IsUnique();

            // Add default timestamp for postgres
            entity
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Persist RefreshTokens as an owned collection
            entity.OwnsMany(u => u.RefreshTokens, rt =>
            {
                rt.WithOwner().HasForeignKey("UserId");
                rt.ToTable("UserRefreshTokens");
                rt.Property(r => r.Token).IsRequired();
                rt.Property(r => r.ExpiresAt).IsRequired();
                rt.Property(r => r.Invalidated).HasDefaultValue(false);
                rt.HasKey("UserId", "Token");
            });
        });

        // Screening(One) - Booking(Many)
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Screening)
            .WithMany(s => s.Bookings)
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
        
        // SeatType table
        modelBuilder.Entity<SeatType>()
            .Property(s => s.PriceAmountDiscount)
                .HasPrecision(5,2)
                .HasDefaultValue(0m);

        // ProjectionType table
        modelBuilder.Entity<ProjectionType>()
            .Property(p => p.PriceAmountDiscount)
                .HasPrecision(5,2)
                .HasDefaultValue(0m);

        // PersonType table
        modelBuilder.Entity<PersonType>()
            .Property(p => p.PricePercentDiscount)
                .HasPrecision(5,2)
                .HasDefaultValue(0m);

        // Offer tables
        modelBuilder.Entity<Offer>(o =>
        {
            o.Property(p => p.IsActive).HasDefaultValue(true);
            o.Property(p => p.IsStackable).HasDefaultValue(true);
            o.Property(p => p.Priority).HasDefaultValue(0);
        });

        modelBuilder.Entity<Offer>()
            .HasMany(o => o.Conditions)
            .WithOne(c => c.Offer)
            .HasForeignKey(c => c.OfferId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Offer>()
            .HasMany(o => o.Effects)
            .WithOne(e => e.Offer)
            .HasForeignKey(e => e.OfferId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Offer>()
            .HasMany(o => o.AppliedOffers)
            .WithOne(a => a.Offer)
            .HasForeignKey(a => a.OfferId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AppliedOffer>()
            .HasOne(a => a.Booking)
            .WithMany()
            .HasForeignKey(a => a.BookingId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_AppliedOffers_Bookings");

        modelBuilder.Entity<OfferEffect>()
            .Property(e => e.EffectValue).HasPrecision(5,2);

        modelBuilder.Entity<AppliedOffer>()
            .Property(a => a.DiscountAmount).HasPrecision(5,2);

        base.OnModelCreating(modelBuilder);
    }
}
