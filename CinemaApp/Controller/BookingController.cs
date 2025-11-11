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

        [HttpPost("initiate")]
        public IActionResult InitiateBooking([FromBody] BookingRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var booking = _service.InitiateBooking(request);
            return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
        }

        [HttpPost("{id:int}/confirm")]
        public IActionResult ConfirmBooking(int id)
        {
            _service.ConfirmBooking(id);
            return NoContent();
        }

        [HttpPost("{id:int}/cancel")]
        public IActionResult CancelBooking(int id)
        {
            _service.CancelBooking(id);
            return NoContent();
        }

        [HttpGet("my-bookings")]
        public IActionResult GetMyBookings([FromQuery] int userId)
        {
            var bookings = _service.GetUserBookings(userId);
            return Ok(bookings);
        }
    }
}