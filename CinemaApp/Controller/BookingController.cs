using CinemaApp.DTO;
using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Controllers
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
            var bookings = _service.GetAll();
            return Ok(bookings);
        }

        [HttpGet("{id:int}")]
        public ActionResult<Booking> Get(int id)
        {
            var booking = _service.Get(id);
            if (booking == null) return NotFound();
            return Ok(booking);
        }

        [HttpPost]
        public IActionResult Create([FromBody] BookingCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var created = _service.Create(dto);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                // validation / missing reference
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex) when (ex.Message.Contains("seat"))
            {
                // seat already taken
                return Conflict(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}