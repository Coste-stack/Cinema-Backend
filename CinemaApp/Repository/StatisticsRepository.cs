
using CinemaApp.Model;
using CinemaApp.Data;
using Microsoft.EntityFrameworkCore;
using CinemaApp.DTO;

namespace CinemaApp.Repository;

public interface IStatisticsRepository
{
    List<PopularMovieDTO> GetPopularMovies(int top);
    List<LatestMovieDTO> GetLatestMovies(int days);
}

public class StatisticsRepository : IStatisticsRepository
{
    private readonly AppDbContext _context;

    public StatisticsRepository(AppDbContext context) => _context = context;

    public List<PopularMovieDTO> GetPopularMovies(int top)
    {
        // Project directly to PopularMovieDTO with ticket counts
        var q = _context.Tickets
            .Include(t => t.Screening) // ensure screening is loaded
            .ThenInclude(s => s!.Movie)
            .Where(t => t.ScreeningId != 0)
            .GroupBy(t => new { 
                t.Screening!.MovieId, 
                t.Screening.Movie.Title, 
                t.Screening.Movie.Description, 
                t.Screening.Movie.Duration, 
                t.Screening.Movie.Rating, 
                t.Screening.Movie.ReleaseDate })
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

    public List<LatestMovieDTO> GetLatestMovies(int days)
    {
        var since = DateTime.UtcNow.Date.AddDays(-days);
        var q = _context.Movies
            .Where(m => m.ReleaseDate != null && m.ReleaseDate >= since)
            .OrderByDescending(m => m.ReleaseDate)
            .Select(m => new LatestMovieDTO {
                Movie = new MovieDto {
                    Id = m.Id,
                    Title = m.Title ?? string.Empty,
                    Description = m.Description,
                    Duration = m.Duration,
                    Rating = m.Rating,
                    ReleaseDate = m.ReleaseDate
                },
                ReleaseDate = m.ReleaseDate
            })
            .ToList();

        return q;
    }
}
