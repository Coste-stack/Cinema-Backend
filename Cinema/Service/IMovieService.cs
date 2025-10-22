using Cinema.Model;

namespace Cinema.Service;

public interface IMovieService
{
    List<Movie> GetAll();
    Movie? Get(int id);
    void Add(Movie movie);
    void Delete(int id);
    void Update(Movie movie);
}