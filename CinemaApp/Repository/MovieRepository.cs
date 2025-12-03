
using CinemaApp.Model;
using CinemaApp.Data;
using Microsoft.EntityFrameworkCore;
using CinemaApp.DTO;

namespace CinemaApp.Repository;

public interface IMovieRepository
{
    List<Movie> GetAll();
    List<MovieFullDto> GetAllFull();
    Movie? GetById(int id);
    Movie Add(Movie movie);
    void Update(Movie movie);
    void Delete(Movie movie);
}

public class MovieRepository : IMovieRepository
{
    private readonly AppDbContext _context;

    public MovieRepository(AppDbContext context) => _context = context;

    public List<Movie> GetAll() => 
        _context.Movies
            .Include(m => m.Genres)
            .ToList();

    public List<MovieFullDto> GetAllFull()
    {
        return _context.Movies
            .Include(m => m.Genres)
            .Include(m => m.Screenings)
                .ThenInclude(s => s.ProjectionType)
            .Select(m => new MovieFullDto
            {
                Id = m.Id,
                Title = m.Title ?? "",
                Description  = m.Description,
                Duration = m.Duration,
                Rating = m.Rating,
                ReleaseDate = m.ReleaseDate,
                Genres = m.Genres
                    .Select(g => g.Name)
                    .ToList(),
                Screenings = m.Screenings
                    .Select(s => new ScreeningDto
                    {
                        Id = s.Id,
                        StartTime = s.StartTime,
                        Language = s.Language,
                        ProjectionType = s.ProjectionType.Name
                    })
                    .ToList()
            })
            .ToList();
    } 

    public Movie? GetById(int id) => _context.Movies.Include(m => m.Genres).FirstOrDefault(m => m.Id == id);

    public Movie Add(Movie movie)
    {
        _context.Movies.Add(movie);
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new ConflictException("No rows affected when adding a movie.");
            return movie;
        }
        catch (DbUpdateException ex)
        {
            throw new ConflictException("Database update failed when adding a movie.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Unexpected error when adding a movie.", ex);
        }
    }

    public void Update(Movie movie)
    {
        _context.Movies.Update(movie);
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new ConflictException("No rows affected when updating a movie.");
        }
        catch (DbUpdateException ex)
        {
            throw new ConflictException("Database update failed when updating a movie.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Unexpected error when updating a movie.", ex);
        }
    }

    public void Delete(Movie movie)
    {
        _context.Movies.Remove(movie);
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new ConflictException("No rows affected when deleting a room.");
        }
        catch (DbUpdateException ex)
        {
            throw new ConflictException("Database update failed when deleting a room.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Unexpected error when deleting a room.", ex);
        }
    }
}
