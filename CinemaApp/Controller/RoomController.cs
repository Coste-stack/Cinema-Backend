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
        return _service.GetById(id);
    }

    [HttpPost]
    public ActionResult Create([FromBody] Room room)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = _service.Add(room);
        return CreatedAtAction(nameof(GetById), new { id = existing.Id }, existing);
    }

    [HttpPut("{id}")]
    public ActionResult Update(int id, [FromBody] Room room)
    {   
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _service.Update(id, room);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        _service.Delete(id);
        return NoContent();
    }
}