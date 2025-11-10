using CinemaApp.Data;
using CinemaApp.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace CinemaApp.Tests.Helpers;

/// <summary>
/// Centralized test data seeder for all test classes
/// </summary>
public static class TestDataSeeder
{
    public static AppDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB per test
            .Options;
        return new AppDbContext(options);
    }

    /// <summary>
    /// Seeds all lookup data (SeatTypes, PersonTypes, ProjectionTypes, Genres)
    /// </summary>
    public static void SeedLookupData(AppDbContext context)
    {
        // Seed SeatTypes
        context.SeatTypes.AddRange(
            new SeatType { Id = 1, Name = "Regular", PriceAmountDiscount = 0 },
            new SeatType { Id = 2, Name = "VIP", PriceAmountDiscount = 5 }
        );

        // Seed PersonTypes
        context.PersonTypes.AddRange(
            new PersonType { Id = 1, Name = "Adult", PricePercentDiscount = 0 },
            new PersonType { Id = 2, Name = "Child", PricePercentDiscount = 30 },
            new PersonType { Id = 3, Name = "Student", PricePercentDiscount = 20 }
        );

        // Seed ProjectionTypes
        context.ProjectionTypes.AddRange(
            new ProjectionType { Id = 1, Name = "2D", PriceAmountDiscount = 0 },
            new ProjectionType { Id = 2, Name = "3D", PriceAmountDiscount = 3 }
        );

        // Seed Genres
        context.Genres.AddRange(
            new Genre { Id = 1, Name = "Action" },
            new Genre { Id = 2, Name = "Comedy" },
            new Genre { Id = 3, Name = "Drama" },
            new Genre { Id = 4, Name = "Sci-Fi" }
        );

        context.SaveChanges();
    }

    /// <summary>
    /// Seeds a basic cinema with rooms and seats
    /// </summary>
    public static (Cinema cinema, Room room, List<Seat> seats) SeedCinemaWithRoomAndSeats(
        AppDbContext context,
        string cinemaName = "Test Cinema",
        string roomName = "Room 1",
        int regularSeats = 2,
        int vipSeats = 2)
    {
        var cinema = new Cinema
        {
            Name = cinemaName,
            Address = "123 Test St",
            City = "Test City"
        };
        context.Cinemas.Add(cinema);
        context.SaveChanges();

        var room = new Room
        {
            Name = roomName,
            CinemaId = cinema.Id
        };
        context.Rooms.Add(room);
        context.SaveChanges();

        var seats = new List<Seat>();
        
        // Add regular seats
        for (int i = 1; i <= regularSeats; i++)
        {
            seats.Add(new Seat
            {
                Row = "A",
                Number = i,
                RoomId = room.Id,
                SeatTypeId = 1 // Regular
            });
        }

        // Add VIP seats
        for (int i = 1; i <= vipSeats; i++)
        {
            seats.Add(new Seat
            {
                Row = "B",
                Number = i,
                RoomId = room.Id,
                SeatTypeId = 2 // VIP
            });
        }

        context.Seats.AddRange(seats);
        context.SaveChanges();

        return (cinema, room, seats);
    }

    /// <summary>
    /// Seeds a movie with genres
    /// </summary>
    public static Movie SeedMovie(
        AppDbContext context,
        string title = "Test Movie",
        int duration = 120,
        decimal basePrice = 10.00m,
        List<int>? genreIds = null)
    {
        var movie = new Movie
        {
            Title = title,
            Description = "A test movie description",
            Duration = duration,
            BasePrice = basePrice,
            Rating = MovieRating.PG13,
            ReleaseDate = DateTime.Now.AddMonths(-1)
        };

        if (genreIds != null && genreIds.Count > 0)
        {
            var genres = context.Genres.Where(g => genreIds.Contains(g.Id)).ToList();
            movie.Genres = genres;
        }

        context.Movies.Add(movie);
        context.SaveChanges();

        return movie;
    }

    /// <summary>
    /// Seeds a screening for a movie
    /// </summary>
    public static Screening SeedScreening(
        AppDbContext context,
        int movieId,
        int roomId,
        int projectionTypeId = 1,
        decimal? basePrice = null,
        DateTime? startTime = null)
    {
        var screening = new Screening
        {
            MovieId = movieId,
            RoomId = roomId,
            ProjectionTypeId = projectionTypeId,
            StartTime = startTime ?? DateTime.Now.AddDays(1),
            BasePrice = basePrice ?? 12.00m,
            Language = "English"
        };

        context.Screenings.Add(screening);
        context.SaveChanges();

        return screening;
    }

    /// <summary>
    /// Seeds a user
    /// </summary>
    public static User SeedUser(
        AppDbContext context,
        string email = "test@example.com",
        UserType userType = UserType.Registered)
    {
        var user = new User
        {
            Email = email,
            UserType = userType,
            CreatedAt = DateTime.Now,
            PasswordHash = "hashed_password"
        };

        context.Users.Add(user);
        context.SaveChanges();

        return user;
    }

    /// <summary>
    /// Seeds basic cinema infrastructure (cinema + room)
    /// </summary>
    public static (Cinema cinema, Room room) SeedCinemaWithRoom(
        AppDbContext context,
        string cinemaName = "Cinema Galaxy",
        string roomName = "Room 1")
    {
        var cinema = new Cinema
        {
            Name = cinemaName,
            Address = "Main Street 10",
            City = "Warsaw"
        };
        context.Cinemas.Add(cinema);
        context.SaveChanges();

        var room = new Room
        {
            Name = roomName,
            CinemaId = cinema.Id
        };
        context.Rooms.Add(room);
        context.SaveChanges();

        return (cinema, room);
    }

    /// <summary>
    /// Seeds multiple cinemas
    /// </summary>
    public static List<Cinema> SeedMultipleCinemas(AppDbContext context)
    {
        var cinemas = new List<Cinema>
        {
            new Cinema { Name = "Cinema Galaxy", Address = "Main Street 10", City = "Warsaw" },
            new Cinema { Name = "MegaFilm", Address = "Broadway 15", City = "Krakow" }
        };

        context.Cinemas.AddRange(cinemas);
        context.SaveChanges();

        return cinemas;
    }

    /// <summary>
    /// Seeds multiple rooms for a cinema
    /// </summary>
    public static List<Room> SeedMultipleRooms(AppDbContext context, int cinemaId)
    {
        var rooms = new List<Room>
        {
            new Room { Name = "Room A", CinemaId = cinemaId },
            new Room { Name = "Room B", CinemaId = cinemaId }
        };

        context.Rooms.AddRange(rooms);
        context.SaveChanges();

        return rooms;
    }

    /// <summary>
    /// Seeds movies with genres for testing
    /// </summary>
    public static List<Movie> SeedMoviesWithGenres(AppDbContext context)
    {
        // Ensure genres exist
        var genreSciFi = context.Genres.Find(1) ?? new Genre { Id = 1, Name = "Sci-Fi" };
        var genreAction = context.Genres.Find(2) ?? new Genre { Id = 2, Name = "Action" };
        var genreThriller = context.Genres.Find(3) ?? new Genre { Id = 3, Name = "Thriller" };

        if (genreSciFi.Id == 0) context.Genres.Add(genreSciFi);
        if (genreAction.Id == 0) context.Genres.Add(genreAction);
        if (genreThriller.Id == 0) context.Genres.Add(genreThriller);
        context.SaveChanges();

        var movies = new List<Movie>
        {
            new Movie
            {
                Title = "Inception",
                Description = "A mind-bending thriller about dreams within dreams.",
                Duration = 148,
                Rating = MovieRating.PG13,
                ReleaseDate = new DateTime(2010, 7, 16),
                Genres = new List<Genre> { genreSciFi, genreThriller }
            },
            new Movie
            {
                Title = "The Dark Knight",
                Description = "Batman faces the Joker in Gotham City.",
                Duration = 152,
                Rating = MovieRating.PG13,
                ReleaseDate = new DateTime(2008, 7, 18),
                Genres = new List<Genre> { genreAction, genreThriller }
            }
        };

        context.Movies.AddRange(movies);
        context.SaveChanges();

        return movies;
    }

    /// <summary>
    /// Seeds a genre (for test scenarios requiring specific genres)
    /// </summary>
    public static Genre SeedGenre(AppDbContext context, string name)
    {
        var genre = new Genre { Name = name };
        context.Genres.Add(genre);
        context.SaveChanges();
        return genre;
    }

    /// <summary>
    /// Seeds users for authentication testing
    /// </summary>
    public static (User guest, User registered) SeedUsersForAuthTests(AppDbContext context, Microsoft.AspNetCore.Identity.PasswordHasher<User> passwordHasher)
    {
        var guest = new User
        {
            Email = "guest@example.com",
            UserType = UserType.Guest
        };

        var registered = new User
        {
            Email = "reg@example.com",
            UserType = UserType.Registered
        };
        registered.PasswordHash = passwordHasher.HashPassword(registered, "InitialPass1!");

        context.Users.AddRange(guest, registered);
        context.SaveChanges();

        return (guest, registered);
    }

    /// <summary>
    /// Seeds complete test environment with all entities
    /// </summary>
    public static TestEnvironment SeedCompleteEnvironment(AppDbContext context)
    {
        SeedLookupData(context);

        var (cinema, room, seats) = SeedCinemaWithRoomAndSeats(context);
        var movie = SeedMovie(context, genreIds: new List<int> { 1, 4 }); // Action, Sci-Fi
        var screening = SeedScreening(context, movie.Id, room.Id);
        var user = SeedUser(context);

        return new TestEnvironment
        {
            Cinema = cinema,
            Room = room,
            Seats = seats,
            Movie = movie,
            Screening = screening,
            User = user
        };
    }
}

/// <summary>
/// Contains all seeded test data for easy access
/// </summary>
public class TestEnvironment
{
    public Cinema Cinema { get; set; } = null!;
    public Room Room { get; set; } = null!;
    public List<Seat> Seats { get; set; } = new();
    public Movie Movie { get; set; } = null!;
    public Screening Screening { get; set; } = null!;
    public User User { get; set; } = null!;
}
