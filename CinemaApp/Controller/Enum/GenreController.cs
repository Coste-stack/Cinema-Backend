using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenreController : LookupController<Genre>
{
    public GenreController(ILookupService<Genre> service) : base(service) { }
}
