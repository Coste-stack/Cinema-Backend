using CinemaApp.DTO;
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
        return _service.GetById(id);
    }

    [HttpGet("movie/{movieId:int}")]
    public ActionResult<List<Screening>> GetByMovie(int movieId)
    {
        return _service.GetByMovie(movieId);
    }

    [HttpGet("room/{roomId:int}")]
    public ActionResult<List<Screening>> GetByRoom(int roomId)
    {
        return _service.GetByRoom(roomId);
    }

    [HttpGet("{id}/available-seats")]
    public ActionResult<List<Seat>> GetAvailableSeats(int id)
    {
        var seats = _service.GetAvailableSeats(id);
        return seats;
    }

    [HttpGet("{id}/seat-map")]
    public ActionResult<SeatMapResponseDTO> GetSeatMap(int id)
    {
        var seatMap = _service.GetSeatMapWithAvailability(id);
        return seatMap;
    }

    [HttpPost]
    public ActionResult Create([FromBody] Screening screening)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = _service.Add(screening);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public ActionResult Update(int id, [FromBody] Screening screening)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _service.Update(id, screening);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteById(int id)
    {
        _service.Delete(id);
        return NoContent();
    }

    [HttpDelete("movie/{movieId:int}")]
    public ActionResult DeleteByMovie(int movieId)
    {
        _service.DeleteByMovie(movieId);
        return NoContent();
    }

    [HttpDelete("room/{roomId:int}")]
    public ActionResult DeleteByRoom(int roomId)
    {
        _service.DeleteByRoom(roomId);
        return NoContent();
    }
}