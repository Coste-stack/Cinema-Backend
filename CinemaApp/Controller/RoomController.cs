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
    private readonly ICinemaService _cinemaService;

    public RoomController(IRoomService service, ICinemaService cinemaService)
    {
        _service = service;
        _cinemaService = cinemaService;
    }


    [HttpGet]
    public ActionResult<List<Room>> GetAll()
    {
        return _service.GetAll();
    }

    [HttpGet("{id}")]
    public ActionResult<Room> GetById(int id)
    {
        Room? room = _service.GetById(id);
        if (room == null) return NotFound();
        return room;
    }

    [HttpPost]
    public IActionResult Create([FromBody] Room room)
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
    public IActionResult Update(int id, [FromBody] Room room)
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
    public IActionResult Delete(int id)
    {
        Room? room = _service.GetById(id);
        if (room == null) return NotFound();
        
        _service.Delete(id);
        return NoContent();
    }
}