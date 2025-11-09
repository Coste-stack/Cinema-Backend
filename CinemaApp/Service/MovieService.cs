using CinemaApp.Model;
using CinemaApp.Repository;

namespace CinemaApp.Service;

public interface IMovieService
{
    List<Movie> GetAll();
    Movie GetById(int id);
    Movie Add(Movie movie);
    void Update(int id, Movie movie);
    void Delete(int id);
}

public class MovieService : IMovieService
{
    private readonly IMovieRepository _repository;

    public MovieService(IMovieRepository repository) => _repository = repository;

    public List<Movie> GetAll()
    {
        return _repository.GetAll().ToList();
    }

    public Movie GetById(int id)
    {
        var movie = _repository.GetById(id);
        if (movie == null)
            throw new NotFoundException($"Movie with ID {id} not found.");
        return movie;
    }

    public Movie Add(Movie movie)
    {
        if (movie.Genres == null || !movie.Genres.Any())
            throw new BadRequestException("At least one genre must be specified for the movie.");
            
        return _repository.Add(movie);
    }

    public void Update(int id, Movie movie)
    {
        if (id != movie.Id)
            throw new BadRequestException($"ID {id} and ID {movie.Id} mismatch in request objects");

        var existing = _repository.GetById(id);
        if (existing == null) 
            throw new NotFoundException($"Movie with ID {id} not found.");

        if (!string.IsNullOrWhiteSpace(movie.Title))
            existing.Title = movie.Title.Trim();

        if (!string.IsNullOrEmpty(movie.Description))
            existing.Description = movie.Description;

        if (movie.Duration >= 0)
            existing.Duration = movie.Duration;

        if (movie.BasePrice >= 0)
            existing.BasePrice = movie.BasePrice;

        if (movie.Genres != null && movie.Genres.Any())
        {
            existing.Genres.Clear();
            foreach (var genre in movie.Genres)
            {
                existing.Genres.Add(genre);
            }
        }
        
        // Ensure at least one genre remains
        if (!existing.Genres.Any())
            throw new BadRequestException("At least one genre must be specified for the movie.");

        if (movie.Rating != null)
            existing.Rating = movie.Rating;

        if (movie.ReleaseDate != null)
            existing.ReleaseDate = movie.ReleaseDate;

        _repository.Update(existing);
    }

    public void Delete(int id)
    {
        var movie = _repository.GetById(id);
        if (movie == null) 
            throw new NotFoundException($"Movie with ID {id} not found.");

        _repository.Delete(movie);
    }
}