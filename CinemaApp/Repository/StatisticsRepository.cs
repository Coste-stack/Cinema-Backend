
using CinemaApp.Model;
using CinemaApp.Data;
using Microsoft.EntityFrameworkCore;
using CinemaApp.DTO;

namespace CinemaApp.Repository;

public interface IStatisticsRepository
{
    List<PopularMovieDTO> GetPopularMovies(int top);
    List<MovieDto> GetLatestMovies(int days);
}

public class StatisticsRepository : IStatisticsRepository
{
    private readonly AppDbContext _context;

    public StatisticsRepository(AppDbContext context) => _context = context;

    public List<PopularMovieDTO> GetPopularMovies(int top)
    {
        // Project directly to PopularMovieDTO with ticket counts
        var q = _context.Tickets
            .Include(t => t.Booking)
                .ThenInclude(b => b.Screening)
                    .ThenInclude(s => s!.Movie)
            // ensure tickets are attached to bookings with screenings
            .Where(t => t.Booking != null && t.Booking.ScreeningId != 0)
            .GroupBy(t => new {
                MovieId = t.Booking!.Screening!.MovieId,
                Title = t.Booking.Screening.Movie.Title,
                Description = t.Booking.Screening.Movie.Description,
                Duration = t.Booking.Screening.Movie.Duration,
                Rating = t.Booking.Screening.Movie.Rating,
                ReleaseDate = t.Booking.Screening.Movie.ReleaseDate
            })
            .Select(g => new PopularMovieDTO {
                Movie = new MovieDto {
                    Id = g.Key.MovieId,
                    Title = g.Key.Title ?? string.Empty,
                    Description = g.Key.Description,
                    Duration = g.Key.Duration,
                    Rating = g.Key.Rating,
                    ReleaseDate = g.Key.ReleaseDate
                },
                TicketsSold = g.Count()
            })
            .OrderByDescending(x => x.TicketsSold)
            .Take(top)
            .ToList();

        return q;
    } 

    public List<MovieDto> GetLatestMovies(int days)
    {
        var since = DateTime.UtcNow.Date.AddDays(-days);
        var q = _context.Movies
            .Where(m => m.ReleaseDate != null && m.ReleaseDate >= since)
            .OrderByDescending(m => m.ReleaseDate)
            .Select(m => new MovieDto {
                Id = m.Id,
                Title = m.Title ?? string.Empty,
                Description = m.Description,
                Duration = m.Duration,
                Rating = m.Rating,
                ReleaseDate = m.ReleaseDate
            })
            .ToList();

        return q;
    }
}
