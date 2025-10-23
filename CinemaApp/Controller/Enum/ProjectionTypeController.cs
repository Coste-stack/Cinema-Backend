using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectionTypesController : ControllerBase
{
    private readonly IProjectionTypeService _service;

    public ProjectionTypesController(IProjectionTypeService service)
    {
        _service = service;
    }

    [HttpGet]
    public ActionResult<List<ProjectionType>> GetAll() =>
        _service.GetAll();

    [HttpGet("{id}")]
    public ActionResult<ProjectionType> GetById(int id)
    {
        var projectionType = _service.GetById(id);
        if (projectionType == null) return NotFound();
        return Ok(projectionType);
    }

    [HttpPost]
    public ActionResult Create([FromBody] ProjectionType projectionType)
    {
        _service.Create(projectionType);
        return CreatedAtAction(nameof(GetById), new { id = projectionType.Id }, projectionType);
    }

    [HttpPut("{id}")]
    public ActionResult Update(int id, [FromBody] ProjectionType projectionType)
    {
        if (id != projectionType.Id)
            return BadRequest("ID mismatch");
            
        ProjectionType? existing = _service.GetById(id);
            if(existing == null) return NotFound();

        _service.Update(projectionType);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        ProjectionType? existing = _service.GetById(id);
        if (existing == null) return NotFound();

        _service.Delete(id);
        return NoContent();
    }
}
