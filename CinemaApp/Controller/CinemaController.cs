using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controller;

[ApiController]
[Route("[controller]")]
public class CinemaController(ICinemaService service) : ControllerBase
{
    private readonly ICinemaService _service = service;

    [HttpGet]
    public ActionResult<List<Cinema>> GetAll() =>
        _service.GetAll();

    [HttpGet("{id}")]
    public ActionResult<Cinema> Get(int id)
    {
        var cinema = _service.Get(id);

        if (cinema == null) return NotFound();
        return cinema;
    }

    [HttpPost]
    public IActionResult Create([FromBody] Cinema cinema)
    {
        _service.Add(cinema);
        return CreatedAtAction(nameof(Get), new { id = cinema.Id }, cinema);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Cinema cinema)
    {
        if (id != cinema.Id)
        return BadRequest();
            
        var existingCinema = _service.Get(id);
        if(existingCinema == null) return NotFound();
    
        _service.Update(cinema);           
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var cinema = _service.Get(id);
   
        if (cinema == null) return NotFound();
        
        _service.Delete(id);
        return NoContent();
    }
}