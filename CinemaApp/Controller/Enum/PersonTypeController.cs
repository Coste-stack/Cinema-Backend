using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonTypeController : LookupController<PersonType>
{
    public PersonTypeController(ILookupService<PersonType> service) : base(service) { }
}
