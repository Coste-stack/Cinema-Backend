using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectionTypeController : LookupController<ProjectionType>
{
    public ProjectionTypeController(ILookupService<ProjectionType> service) : base(service) { }
}
