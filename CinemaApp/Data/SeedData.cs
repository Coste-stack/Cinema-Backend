using CinemaApp.Model;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context, ILogger logger)
    {
        try
        {
            // Seed lookup tables (always needed)
            await SeedLookupTablesAsync(context, logger);

            // Seed template data for frontend development
            await SeedTemplateDataAsync(context, logger);

            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static async Task SeedLookupTablesAsync(AppDbContext context, ILogger logger)
    {
        // Seed SeatTypes
        if (!await context.SeatTypes.AnyAsync())
        {
            context.SeatTypes.AddRange(
                new SeatType { Id = 1, Name = "Regular", PriceAmountDiscount = 0 },
                new SeatType { Id = 2, Name = "VIP", PriceAmountDiscount = 5 },
                new SeatType { Id = 3, Name = "Premium", PriceAmountDiscount = 3 }
            );
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded SeatTypes.");
        }

        // Seed PersonTypes
        if (!await context.PersonTypes.AnyAsync())
        {
            context.PersonTypes.AddRange(
                new PersonType { Id = 1, Name = "Adult", PricePercentDiscount = 0 },
                new PersonType { Id = 2, Name = "Child", PricePercentDiscount = 30 },
                new PersonType { Id = 3, Name = "Student", PricePercentDiscount = 20 },
                new PersonType { Id = 4, Name = "Senior", PricePercentDiscount = 25 }
            );
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded PersonTypes.");
        }

        // Seed ProjectionTypes
        if (!await context.ProjectionTypes.AnyAsync())
        {
            context.ProjectionTypes.AddRange(
                new ProjectionType { Id = 1, Name = "2D", PriceAmountDiscount = 0 },
                new ProjectionType { Id = 2, Name = "3D", PriceAmountDiscount = 3 },
                new ProjectionType { Id = 3, Name = "IMAX", PriceAmountDiscount = 5 },
                new ProjectionType { Id = 4, Name = "4DX", PriceAmountDiscount = 7 }
            );
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded ProjectionTypes.");
        }

        // Seed Genres
        if (!await context.Genres.AnyAsync())
        {
            context.Genres.AddRange(
                new Genre { Id = 1, Name = "Action" },
                new Genre { Id = 2, Name = "Comedy" },
                new Genre { Id = 3, Name = "Drama" },
                new Genre { Id = 4, Name = "Horror" },
                new Genre { Id = 5, Name = "Sci-Fi" },
                new Genre { Id = 6, Name = "Romance" },
                new Genre { Id = 7, Name = "Thriller" },
                new Genre { Id = 8, Name = "Animation" },
                new Genre { Id = 9, Name = "Adventure" },
                new Genre { Id = 10, Name = "Fantasy" }
            );
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded Genres.");
        }
    }

    private static async Task SeedTemplateDataAsync(AppDbContext context, ILogger logger)
    {
        // Seed Cinemas
        var cinemas = await SeedCinemasAsync(context, logger);
        
        // Seed Movies
        var movies = await SeedMoviesAsync(context, logger);
        
        // Seed Rooms and Seats for each cinema
        var rooms = await SeedRoomsAndSeatsAsync(context, cinemas, logger);
        
        // Seed Screenings
        await SeedScreeningsAsync(context, movies, rooms, logger);
    }

    private static async Task<List<Cinema>> SeedCinemasAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Cinemas.AnyAsync())
        {
            return await context.Cinemas.ToListAsync();
        }

        var cinemas = new List<Cinema>
        {
            new Cinema
            {
                Name = "Cineplex Downtown",
                Address = "123 Main Street",
                City = "New York"
            },
            new Cinema
            {
                Name = "Starlight Cinemas",
                Address = "456 Park Avenue",
                City = "Los Angeles"
            },
            new Cinema
            {
                Name = "Grand Theater",
                Address = "789 Broadway",
                City = "Chicago"
            }
        };

        context.Cinemas.AddRange(cinemas);
        await context.SaveChangesAsync();
        logger.LogInformation($"Seeded {cinemas.Count} cinemas.");
        
        return cinemas;
    }

    private static async Task<List<Movie>> SeedMoviesAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Movies.AnyAsync())
        {
            return await context.Movies.Include(m => m.Genres).ToListAsync();
        }

        // Get all available genres
        var allGenres = await context.Genres.ToListAsync();
        
        if (allGenres.Count == 0)
        {
            logger.LogError("No genres found in database. Ensure lookup tables are seeded first.");
            throw new InvalidOperationException("No genres found.");
        }

        var movies = new List<Movie>
        {
            new Movie
            {
                Title = "Galactic Warriors",
                Description = "An epic space adventure across the galaxy.",
                Duration = 148,
                Rating = MovieRating.PG13,
                ReleaseDate = new DateTime(2024, 6, 15),
                BasePrice = 12.00m,
                Genres = allGenres.Where(g => g.Name == "Action" || g.Name == "Sci-Fi").ToList()
            },
            new Movie
            {
                Title = "The Last Stand",
                Description = "A gripping tale of survival and courage.",
                Duration = 165,
                Rating = MovieRating.R,
                ReleaseDate = new DateTime(2024, 8, 20),
                BasePrice = 13.00m,
                Genres = allGenres.Where(g => g.Name == "Action" || g.Name == "Drama").ToList()
            },
            new Movie
            {
                Title = "Laugh Out Loud",
                Description = "The funniest comedy of the year!",
                Duration = 105,
                Rating = MovieRating.PG13,
                ReleaseDate = new DateTime(2024, 9, 10),
                BasePrice = 10.00m,
                Genres = allGenres.Where(g => g.Name == "Comedy").ToList()
            },
            new Movie
            {
                Title = "Ocean Dreams",
                Description = "A touching story about life and love.",
                Duration = 120,
                Rating = MovieRating.PG,
                ReleaseDate = new DateTime(2024, 7, 5),
                BasePrice = 11.00m,
                Genres = allGenres.Where(g => g.Name == "Drama").ToList()
            },
            new Movie
            {
                Title = "Dragon Tales",
                Description = "An animated adventure for the whole family.",
                Duration = 95,
                Rating = MovieRating.G,
                ReleaseDate = new DateTime(2024, 10, 1),
                BasePrice = 9.00m,
                Genres = allGenres.Where(g => g.Name == "Animation").ToList()
            },
            new Movie
            {
                Title = "Night Terrors",
                Description = "A bone-chilling horror experience.",
                Duration = 110,
                Rating = MovieRating.R,
                ReleaseDate = new DateTime(2024, 10, 31),
                BasePrice = 12.00m,
                Genres = allGenres.Where(g => g.Name == "Horror").ToList()
            },
            new Movie
            {
                Title = "Time Paradox",
                Description = "A mind-bending sci-fi thriller.",
                Duration = 140,
                Rating = MovieRating.PG13,
                ReleaseDate = new DateTime(2025, 1, 15),
                BasePrice = 13.00m,
                Genres = allGenres.Where(g => g.Name == "Sci-Fi").ToList()
            }
        };

        // Ensure each movie has at least one genre (use first available if specific not found)
        foreach (var movie in movies)
        {
            if (movie.Genres.Count == 0)
            {
                movie.Genres.Add(allGenres[0]);
            }
        }

        context.Movies.AddRange(movies);
        await context.SaveChangesAsync();
        logger.LogInformation($"Seeded {movies.Count} movies.");
        
        return movies;
    }

    private static async Task<List<Room>> SeedRoomsAndSeatsAsync(AppDbContext context, List<Cinema> cinemas, ILogger logger)
    {
        if (await context.Rooms.AnyAsync())
        {
            return await context.Rooms.Include(r => r.Seats).ToListAsync();
        }

        var rooms = new List<Room>();
        var seatTypes = await context.SeatTypes.ToListAsync();
        
        if (seatTypes.Count == 0)
        {
            logger.LogError("No seat types found in database.");
            throw new InvalidOperationException("No seat types found.");
        }

        // Get seat types by index (first available, second available, etc.)
        var regularSeatTypeId = seatTypes[0].Id;
        var vipSeatTypeId = seatTypes.Count > 1 ? seatTypes[1].Id : seatTypes[0].Id;
        var premiumSeatTypeId = seatTypes.Count > 2 ? seatTypes[2].Id : seatTypes[0].Id;

        foreach (var cinema in cinemas)
        {
            // Standard Room (8 rows x 12 seats, mostly regular)
            var standardRoom = new Room
            {
                Name = "Standard Hall 1",
                CinemaId = cinema.Id
            };

            for (int row = 0; row < 8; row++)
            {
                var rowLetter = (char)('A' + row);
                for (int seat = 1; seat <= 12; seat++)
                {
                    // Last 2 rows are VIP
                    var seatType = row >= 6 ? vipSeatTypeId : regularSeatTypeId;
                    
                    standardRoom.Seats.Add(new Seat
                    {
                        Row = rowLetter.ToString(),
                        Number = seat,
                        SeatTypeId = seatType
                    });
                }
            }
            rooms.Add(standardRoom);

            // IMAX Room (10 rows x 15 seats, premium)
            var imaxRoom = new Room
            {
                Name = "IMAX",
                CinemaId = cinema.Id
            };

            for (int row = 0; row < 10; row++)
            {
                var rowLetter = (char)('A' + row);
                for (int seat = 1; seat <= 15; seat++)
                {
                    // Middle rows (3-7) are premium
                    var seatType = row >= 3 && row <= 7 ? premiumSeatTypeId : regularSeatTypeId;
                    
                    imaxRoom.Seats.Add(new Seat
                    {
                        Row = rowLetter.ToString(),
                        Number = seat,
                        SeatTypeId = seatType
                    });
                }
            }
            rooms.Add(imaxRoom);

            // VIP Room (5 rows x 8 seats, all VIP)
            var vipRoom = new Room
            {
                Name = "VIP Lounge",
                CinemaId = cinema.Id
            };

            for (int row = 0; row < 5; row++)
            {
                var rowLetter = (char)('A' + row);
                for (int seat = 1; seat <= 8; seat++)
                {
                    vipRoom.Seats.Add(new Seat
                    {
                        Row = rowLetter.ToString(),
                        Number = seat,
                        SeatTypeId = vipSeatTypeId
                    });
                }
            }
            rooms.Add(vipRoom);
        }

        context.Rooms.AddRange(rooms);
        await context.SaveChangesAsync();
        logger.LogInformation($"Seeded {rooms.Count} rooms with seats.");
        
        return rooms;
    }

    private static async Task SeedScreeningsAsync(AppDbContext context, List<Movie> movies, List<Room> rooms, ILogger logger)
    {
        if (await context.Screenings.AnyAsync())
        {
            logger.LogInformation("Screenings already exist, skipping seeding.");
            return;
        }

        var screenings = new List<Screening>();
        var projectionTypes = await context.ProjectionTypes.ToListAsync();
        var today = DateTime.UtcNow.Date;

        // Create screenings for the next 7 days
        for (int day = 0; day < 7; day++)
        {
            var screeningDate = today.AddDays(day);

            foreach (var movie in movies)
            {
                // Each movie gets 3-4 screenings per day across different rooms
                var timeslots = new[] { 10, 14, 18, 21 }; // 10 AM, 2 PM, 6 PM, 9 PM
                
                for (int i = 0; i < 3; i++)
                {
                    var room = rooms[Random.Shared.Next(rooms.Count)];
                    var projectionType = projectionTypes[Random.Shared.Next(projectionTypes.Count)];
                    var startTime = screeningDate.AddHours(timeslots[i]);

                    screenings.Add(new Screening
                    {
                        MovieId = movie.Id,
                        RoomId = room.Id,
                        StartTime = startTime,
                        ProjectionTypeId = projectionType.Id,
                        BasePrice = movie.BasePrice + Random.Shared.Next(0, 3) // Slight variation
                    });
                }
            }
        }

        context.Screenings.AddRange(screenings);
        await context.SaveChangesAsync();
        logger.LogInformation($"Seeded {screenings.Count} screenings.");
    }
}
