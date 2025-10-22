using CinemaApp.Model;
using CinemaApp.Repository;

namespace CinemaApp.Service;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _repository;

    public MovieService(IMovieRepository repository) => _repository = repository;

    public List<Movie> GetAll()
    {
        return _repository.GetAll().ToList();
    }

    public Movie? Get(int id)
    {
        return _repository.GetById(id);
    } 

    public void Add(Movie movie)
    {
        _repository.Add(movie);
    }

    public void Delete(int id)
    {
        _repository.Delete(id);
    }

    public void Update(Movie movie)
    {
        _repository.Update(movie);
    }
}