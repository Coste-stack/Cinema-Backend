using CinemaApp.DTO;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controller;

[ApiController]
[Route("[controller]")]
    public class StatisticsController(IStatisticsService service, CinemaApp.Data.AppDbContext db) : ControllerBase
{
    private readonly IStatisticsService _service = service;
        private readonly CinemaApp.Data.AppDbContext _db = db;

    [HttpGet("popular-movies")]
    public ActionResult<List<PopularMovieDTO>> GetPopularMovies([FromQuery] int top = 5) 
    {
        return _service.GetPopularMovies(top);
    }

    [HttpGet("latest-movies")]
    public ActionResult<List<LatestMovieDTO>> GetLatestMovies([FromQuery] int days = 365)
    {
        return _service.GetLatestMovies(days);
    }
        
    [HttpGet("counts")]
    public ActionResult<object> Counts()
    {
        var movies = _db.Movies.Count();
        var screenings = _db.Screenings.Count();
        var tickets = _db.Tickets.Count();
        return Ok(new { Movies = movies, Screenings = screenings, Tickets = tickets });
    }
}