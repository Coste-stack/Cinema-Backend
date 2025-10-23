using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controller;

[ApiController]
[Route("[controller]")]
public class ScreeningController : ControllerBase
{
    private readonly IScreeningService _screeningService;
    private readonly IMovieService _movieService;
    private readonly IRoomService _roomService;

    public ScreeningController(IScreeningService screeningService, IMovieService movieService, IRoomService roomService)
    {
        _screeningService = screeningService;
        _movieService = movieService;
        _roomService = roomService;
    }


    [HttpGet]
    public ActionResult<List<Screening>> GetAll() =>
        _screeningService.GetAll();

    [HttpGet("{id}")]
    public ActionResult<Screening> GetById(int id)
    {
        Screening? screening = _screeningService.Get(id);
        if (screening == null) return NotFound();

        return screening;
    }

    [HttpGet("movie/{movieId}")]
    public ActionResult<List<Screening>> GetByMovie(int movieId)
    {
        Movie? movie = _movieService.Get(movieId);
        if (movie == null) return NotFound($"Movie {movieId} not found.");

        return _screeningService.GetAll().Where(s => s.MovieId == movieId).ToList(); ;
    }

    [HttpGet("room/{roomId}")]
    public ActionResult<List<Screening>> GetByRoom(int roomId)
    {
        Room? room = _roomService.Get(roomId);
        if (room == null) return NotFound($"Room {roomId} not found.");

        return _screeningService.GetAll().Where(s => s.RoomId == roomId).ToList(); ;
    }

    [HttpPost]
    public IActionResult Create([FromBody] Screening screening)
    {
        IActionResult? validateRes = Validate(screening);
        if (validateRes != null) return validateRes;

        _screeningService.Add(screening);
        return CreatedAtAction(nameof(GetById), new { id = screening.Id }, screening);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Screening screening)
    {
        if (id != screening.Id)
        return BadRequest();
            
        Screening? existingScreening = _screeningService.Get(id);
        if (existingScreening == null) return NotFound();
        
        IActionResult? validateRes = Validate(screening);
        if (validateRes != null) return validateRes;
    
        _screeningService.Update(screening);           
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteById(int id)
    {
        Screening? screening = _screeningService.Get(id);
        if (screening == null) return NotFound();

        _screeningService.Delete(id);
        return NoContent();
    }

    [HttpDelete("movie/{movieId}")]
    public IActionResult DeleteByMovie(int movieId)
    {
        Movie? movie = _movieService.Get(movieId);
        if (movie == null) return NotFound($"Movie {movieId} not found.");

        List<Screening> screenings = _screeningService.GetAll().Where(s => s.MovieId == movieId).ToList();
        if (screenings.Count == 0)
            return NotFound($"No screenings found for movie {movieId}.");

        foreach (Screening screening in screenings)
        {
            _screeningService.Delete(screening.Id);
        }

        return NoContent();
    }

    [HttpDelete("room/{roomId}")]
    public IActionResult DeleteByRoom(int roomId)
    {
        Room? room = _roomService.Get(roomId);
        if (room == null) return NotFound($"Room {roomId} not found.");

        List<Screening> screenings = _screeningService.GetAll().Where(s => s.RoomId == roomId).ToList();
        if (screenings.Count == 0)
            return NotFound($"No screenings found for room {roomId}.");

        foreach (Screening screening in screenings)
        {
            _screeningService.Delete(screening.Id);
        }

        return NoContent();
    }
    
    private IActionResult? Validate(Screening screening)
    {
        Movie? movie = _movieService.Get(screening.MovieId);
        if (movie == null)
            return NotFound($"Movie with ID {screening.MovieId} not found.");
            
        Room? room = _roomService.Get(screening.RoomId);
        if (room == null)
            return NotFound($"Room with ID {screening.RoomId} not found.");

        // Add movie duration to screening datetime
        screening.EndTime = screening.StartTime.AddMinutes(movie.Duration);

        return null;
    }
}