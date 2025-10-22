
using Cinema.Model;
using Cinema.Data;

namespace Cinema.Repository;

public class MovieRepository : IMovieRepository
{
    private readonly AppDbContext _context;

    public MovieRepository(AppDbContext context) => _context = context;

    public List<Movie> GetAll() => _context.Movies.ToList();

    public Movie? GetById(int id) => _context.Movies.Find(id);

    public void Add(Movie movie)
    {
        _context.Movies.Add(movie);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var movie = _context.Movies.Find(id);
        if (movie == null) return;

        _context.Movies.Remove(movie);
        _context.SaveChanges();
    }

    public void Update(Movie movie)
    {
        Movie? existingMovie = _context.Movies.Find(movie.Id);
        if (existingMovie == null) return;

        if (!string.IsNullOrEmpty(movie.Title))
            existingMovie.Title = movie.Title;

        if (!string.IsNullOrEmpty(movie.Description))
            existingMovie.Description = movie.Description;

        if (movie.Duration > 0)
            existingMovie.Duration = movie.Duration;

        if (!string.IsNullOrEmpty(movie.Genre))
            existingMovie.Genre = movie.Genre;

        if (movie.Rating != null)
            existingMovie.Rating = movie.Rating;

        if (movie.ReleaseDate != null)
            existingMovie.ReleaseDate = movie.ReleaseDate;

        _context.SaveChanges();
    }
}
