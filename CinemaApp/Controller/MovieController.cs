using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controller;

[ApiController]
[Route("[controller]")]
public class MovieController(IMovieService service) : ControllerBase
{
    private readonly IMovieService _service = service;

    [HttpGet]
    public ActionResult<List<Movie>> GetAll() =>
        _service.GetAll();

    [HttpGet("{id}")]
    public ActionResult<Movie> Get(int id)
    {
        var movie = _service.Get(id);

        if (movie == null) return NotFound();
        return movie;
    }

    [HttpPost]
    public IActionResult Create([FromBody] Movie movie)
    {
        _service.Add(movie);
        return CreatedAtAction(nameof(Get), new { id = movie.Id }, movie);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Movie movie)
    {
        if (id != movie.Id)
        return BadRequest();
            
        var existingMovie = _service.Get(id);
        if(existingMovie == null) return NotFound();
    
        _service.Update(movie);           
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var movie = _service.Get(id);
   
        if (movie == null) return NotFound();
        
        _service.Delete(id);
        return NoContent();
    }
}