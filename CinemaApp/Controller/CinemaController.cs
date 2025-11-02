using CinemaApp.Model;
using CinemaApp.DTO;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Controller;

[ApiController]
[Route("[controller]")]
public class CinemaController(ICinemaService service) : ControllerBase
{
    private readonly ICinemaService _service = service;

    [HttpGet]
    public ActionResult<List<Cinema>> GetAll()
    {
        return _service.GetAll();
    }
        
    [HttpGet("{id}")]
    public ActionResult<Cinema> GetById(int id)
    {
        return _service.GetById(id);
    }

    [HttpPost]
    public ActionResult Create([FromBody] Cinema cinema)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = _service.Add(cinema);
        return CreatedAtAction(nameof(GetById), new { id = existing.Id }, existing);
    }

    [HttpPut("{id}")]
    public ActionResult Update(int id, [FromBody] Cinema cinema)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _service.Update(id, cinema);
        return NoContent();
    }
}