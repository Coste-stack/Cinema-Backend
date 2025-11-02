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
        return _service.GetByRoom(roomId);
    }

    [HttpPost("room/{roomId}/generate")]
    public ActionResult AddSeats(int roomId, [FromQuery] int rows = 5, [FromQuery] int seatsPerRow = 10, [FromQuery] int seatTypeId = 1)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = _service.AddRange(roomId, rows, seatsPerRow, seatTypeId);
        return CreatedAtAction(nameof(AddSeats), existing);
    }

    [HttpDelete("room/{roomId}")]
    public ActionResult DeleteByRoom(int roomId)
    {
        _service.Delete(roomId);
        return NoContent();
    }
}