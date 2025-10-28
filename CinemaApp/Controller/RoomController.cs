using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Controller;

[ApiController]
[Route("[controller]")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _service;
    public RoomController(IRoomService service)
    {
        _service = service;
    }


    [HttpGet]
    public ActionResult<List<Room>> GetAll()
    {
        return _service.GetAll();
    }

    [HttpGet("{id}")]
    public ActionResult<Room> GetById(int id)
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

    [HttpPost]
    public ActionResult Create([FromBody] Room room)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var existing = _service.Add(room);
            return CreatedAtAction(nameof(GetById), new { id = existing.Id }, existing);
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

    [HttpPut("{id}")]
    public ActionResult Update(int id, [FromBody] Room room)
    {   
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            _service.Update(id, room);
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

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
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
}