using CinemaApp.DTO;
using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _service;

        public BookingController(IBookingService service) => _service = service;

        [HttpGet]
        public ActionResult<List<Booking>> GetAll()
        {
            return _service.GetAll();
        }

        [HttpGet("{id:int}")]
        public ActionResult<Booking> GetById(int id)
        {
            return _service.GetById(id);
        }

        [HttpPost]
        public ActionResult Create([FromBody] BookingCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = _service.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
    }
}