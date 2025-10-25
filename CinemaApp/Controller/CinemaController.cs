using CinemaApp.Model;
using CinemaApp.DTO;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Controller;

[ApiController]
[Route("[controller]")]
public class CinemaController(ICinemaService service) : ControllerBase
{
    private readonly ICinemaService _service = service;

    [HttpGet]
    public ActionResult<List<Cinema>> GetAll()
    {
        return _service.GetAll();
    }
        
    [HttpGet("{id}")]
    public ActionResult<Cinema> GetById(int id)
    {
        var cinema = _service.GetById(id);

        if (cinema == null) return NotFound();
        return cinema;
    }

    [HttpPost]
    public ActionResult Create([FromBody] Cinema cinema)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var existing = _service.Add(cinema);
            return CreatedAtAction(nameof(GetById), new { id = existing.Id }, existing);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            return Conflict(new { error = "Database constraint or update error.", details = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { error = "Persistence error.", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public ActionResult Update(int id, [FromBody] Cinema cinema)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            _service.Update(id, cinema);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        } 
        catch (DbUpdateException ex)
        {
            return Conflict(new { error = "Database constraint or update error.", details = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { error = "Persistence error.", details = ex.Message });
        }  
    }
}