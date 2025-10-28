using CinemaApp.DTO;
using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            try
            {
                return _service.GetById(id);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Create([FromBody] BookingCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var created = _service.Create(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new { error = "Persistence error.", details = ex.Message });
            }
        }
    }
}