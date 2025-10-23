using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controller;

[ApiController]
[Route("[controller]")]
public class TicketController(ITicketService service) : ControllerBase
{
    private readonly ITicketService _service = service;

    [HttpGet]
    public ActionResult<List<Ticket>> GetAll()
    {
        var tickets = _service.GetAll();
        return Ok(tickets);
    }
        

    [HttpGet("{id:int}")]
    public ActionResult<Ticket> Get(int id)
    {
        var ticket = _service.Get(id);
        if (ticket == null) return NotFound();
        return Ok(ticket);
    }

    [HttpPost]
    public IActionResult Create([FromBody] Ticket ticket)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var created = _service.Add(ticket);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            // Validation error from service
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            // TODO: Log the exception
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var ticket = _service.Get(id);
        if (ticket == null) return NotFound();
        
        _service.Delete(id);
        return NoContent();
    }
}