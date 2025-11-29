using CinemaApp.DTO;
using CinemaApp.Repository;

namespace CinemaApp.Service;

public interface IStatisticsService
{
    List<PopularMovieDTO> GetPopularMovies(int top);
    List<MovieDto> GetLatestMovies(int days);
}

public class StatisticsService : IStatisticsService
{
    private readonly IStatisticsRepository _repository;

    public StatisticsService(IStatisticsRepository repository) => _repository = repository;

    public List<PopularMovieDTO> GetPopularMovies(int top)
    {
        return _repository.GetPopularMovies(top);
    }

    public List<MovieDto> GetLatestMovies(int days)
    {
        return _repository.GetLatestMovies(days);
    }
}