using CinemaApp.Model;
using CinemaApp.DTO;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controller;

[ApiController]
[Route("[controller]")]
public class MovieController(IMovieService service) : ControllerBase
{
    private readonly IMovieService _service = service;

    [HttpGet]
    public ActionResult GetAll([FromQuery] bool full = false) 
    {
        if (full) {
            return Ok(_service.GetAllFull());
        }
        return Ok(_service.GetAll());
    }

    [HttpGet("{id}")]
    public ActionResult<Movie> GetById(int id)
    {
        return _service.GetById(id);
    }

    [HttpGet("search")]
    public ActionResult Search(
        [FromQuery] string? title,
        [FromQuery] List<int>? genreIds,
        [FromQuery] int? minRating,
        [FromQuery] DateTime? releasedAfter,
        [FromQuery] int? cinemaId,
        [FromQuery] bool? currentlyShowing,
        [FromQuery] bool full = false
    ) {
        return Ok(_service.Search(full, title, genreIds, minRating, releasedAfter, cinemaId, currentlyShowing));
    }

    [HttpPost]
    public ActionResult Create([FromBody] Movie movie)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = _service.Add(movie);
        return CreatedAtAction(nameof(GetById), new { id = movie.Id }, movie);
    }

    [HttpPut("{id}")]
    public ActionResult Update(int id, [FromBody] Movie movie)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _service.Update(id, movie);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        _service.Delete(id);
        return NoContent();
    }
}