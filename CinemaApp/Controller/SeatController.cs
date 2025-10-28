using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Controller;

[ApiController]
[Route("[controller]")]
public class SeatController : ControllerBase
{
    private readonly ISeatService _service;

    public SeatController(ISeatService service)
    {
        _service = service;
    }

    [HttpGet("room/{roomId}")]
    public ActionResult<List<Seat>> GetByRoom(int roomId)
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

    [HttpPost("room/{roomId}/generate")]
    public ActionResult AddSeats(int roomId, [FromQuery] int rows = 5, [FromQuery] int seatsPerRow = 10, [FromQuery] int seatTypeId = 1)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var existing = _service.AddRange(roomId, rows, seatsPerRow, seatTypeId);
            return CreatedAtAction(nameof(AddSeats), existing);
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

    [HttpDelete("room/{roomId}")]
    public ActionResult DeleteByRoom(int roomId)
    {
        try
        {
            _service.Delete(roomId);
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