using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeatTypesController : LookupController<SeatType>
{
    public SeatTypesController(ILookupService<SeatType> service) : base(service) { }
}
