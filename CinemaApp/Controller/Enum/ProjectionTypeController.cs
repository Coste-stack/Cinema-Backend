using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectionTypesController : LookupController<ProjectionType>
{
    public ProjectionTypesController(ILookupService<ProjectionType> service) : base(service) { }
}
