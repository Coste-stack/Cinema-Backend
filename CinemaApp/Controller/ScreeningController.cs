using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Controller;

[ApiController]
[Route("[controller]")]
public class ScreeningController : ControllerBase
{
    private readonly IScreeningService _service;

    public ScreeningController(IScreeningService service)
    {
        _service = service;
    }


    [HttpGet]
    public ActionResult<List<Screening>> GetAll()
    {
        return _service.GetAll();
    }

    [HttpGet("{id:int}")]
    public ActionResult<Screening> GetById(int id)
    {
        try
        {
            return _service.GetById(id);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("movie/{movieId:int}")]
    public ActionResult<List<Screening>> GetByMovie(int movieId)
    {
        try
        {
            return _service.GetByMovie(movieId);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("room/{roomId:int}")]
    public ActionResult<List<Screening>> GetByRoom(int roomId)
    {
        try
        {
            return _service.GetByRoom(roomId);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost]
    public ActionResult Create([FromBody] Screening screening)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var created = _service.Add(screening);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { error = "Persistence error.", details = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public ActionResult Update(int id, [FromBody] Screening screening)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            _service.Update(id, screening);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { error = "Persistence error.", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteById(int id)
    {
        try
        {
            _service.Delete(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { error = "Persistence error.", details = ex.Message });
        } 
    }

    [HttpDelete("movie/{movieId:int}")]
    public ActionResult DeleteByMovie(int movieId)
    {
        try
        {
            _service.DeleteByMovie(movieId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { error = "Persistence error.", details = ex.Message });
        } 
    }

    [HttpDelete("room/{roomId:int}")]
    public ActionResult DeleteByRoom(int roomId)
    {
        try
        {
            _service.DeleteByRoom(roomId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { error = "Persistence error.", details = ex.Message });
        } 
    }
}