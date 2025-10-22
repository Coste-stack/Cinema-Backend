using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controller;

[ApiController]
[Route("[controller]")]
public class SeatController : ControllerBase
{
    private readonly ISeatService _seatService;
    private readonly IRoomService _roomService;

    public SeatController(ISeatService seatService, IRoomService roomService)
    {
        _seatService = seatService;
        _roomService = roomService;
    }

    [HttpGet("room/{roomId}")]
    public ActionResult<List<Seat>> GetByRoom(int roomId)
    {
        Room? room = _roomService.Get(roomId);
        if (room == null) return NotFound($"Room {roomId} not found.");

        return _seatService.GetAll().Where(s => s.RoomId == roomId).ToList();
    }

    [HttpPost("room/{roomId}/generate")]
    public IActionResult GenerateSeats(int roomId, [FromQuery] int rows = 5, [FromQuery] int seatsPerRow = 10)
    {
        Room? room = _roomService.Get(roomId);
        if (room == null) return NotFound($"Cinema with ID {roomId} not found.");

        List<Seat> seatList = new();
        for (char row = 'A'; row < 'A' + rows; row++)
        {
            for (int number = 1; number <= seatsPerRow; number++)
            {
                seatList.Add(new Seat
                {
                    RoomId = roomId,
                    Row = row.ToString(),
                    Number = number
                });
            }
        }

        _seatService.AddRange(seatList);
        return Ok(seatList);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        Seat? seat = _seatService.Get(id);
        if (seat == null) return NotFound($"Seat with ID {id} not found.");

        _seatService.Delete(id);
        return NoContent();
    }
    
    [HttpDelete("room/{roomId}")]
    public IActionResult DeleteByRoom(int roomId)
    {
        Room? room = _roomService.Get(roomId);
        if (room == null) return NotFound($"Room {roomId} not found.");

        List<Seat> seats = _seatService.GetAll().Where(s => s.RoomId == roomId).ToList();
        if (seats.Count == 0) return NotFound("No seats found for this room.");

        foreach (Seat seat in seats)
        {
            _seatService.Delete(seat.Id);
        }

        return NoContent();
    }
}