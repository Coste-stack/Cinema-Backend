using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LookupController<T> : ControllerBase where T : LookupEntity
{
    private readonly ILookupService<T> _service;

    public LookupController(ILookupService<T> service)
    {
        _service = service;
    }

    [HttpGet]
    public ActionResult<List<T>> GetAll() =>
        _service.GetAll();

    [HttpGet("{id}")]
    public ActionResult<T> GetById(int id)
    {
        var projectionType = _service.GetById(id);
        if (projectionType == null) return NotFound();
        return Ok(projectionType);
    }

    [HttpPost]
    public ActionResult Create([FromBody] T projectionType)
    {
        _service.Create(projectionType);
        return CreatedAtAction(nameof(GetById), new { id = projectionType.Id }, projectionType);
    }

    [HttpPut("{id}")]
    public ActionResult Update(int id, [FromBody] T projectionType)
    {
        if (id != projectionType.Id)
            return BadRequest("ID mismatch");
            
        T? existing = _service.GetById(id);
            if(existing == null) return NotFound();

        _service.Update(projectionType);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        T? existing = _service.GetById(id);
        if (existing == null) return NotFound();

        _service.Delete(id);
        return NoContent();
    }
}
