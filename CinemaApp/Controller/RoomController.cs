using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controller;

[ApiController]
[Route("[controller]")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly ICinemaService _cinemaService;

    public RoomController(IRoomService roomService, ICinemaService cinemaService)
    {
        _roomService = roomService;
        _cinemaService = cinemaService;
    }


    [HttpGet]
    public ActionResult<List<Room>> GetAll() =>
        _roomService.GetAll();

    [HttpGet("{id}")]
    public ActionResult<Room> Get(int id)
    {
        Room? room = _roomService.Get(id);
        if (room == null) return NotFound();

        return room;
    }

    [HttpPost]
    public IActionResult Create([FromBody] Room room)
    {
        Cinema? cinema = _cinemaService.GetById(room.CinemaId);
        if (cinema == null) 
            return NotFound($"Cinema with ID {room.CinemaId} not found.");

        _roomService.Add(room);
        return CreatedAtAction(nameof(Get), new { id = room.Id }, room);
    }

    [HttpPost("cinemas/{cinemaId}")]
    public IActionResult Create(int cinemaId, [FromBody] Room room)
    {
        Cinema? cinema = _cinemaService.GetById(cinemaId);
        if (cinema == null) 
            return NotFound($"Cinema with ID {cinemaId} not found.");

        room.CinemaId = cinemaId;
        _roomService.Add(room);
        return CreatedAtAction(nameof(Get), new { id = room.Id }, room);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Room room)
    {
        if (id != room.Id)
        return BadRequest();
            
        Room? existingRoom = _roomService.Get(id);
        if(existingRoom == null) return NotFound();
    
        _roomService.Update(room);           
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        Room? room = _roomService.Get(id);
        if (room == null) return NotFound();
        
        _roomService.Delete(id);
        return NoContent();
    }
}