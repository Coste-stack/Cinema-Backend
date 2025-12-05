using CinemaApp.DTO;
using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace CinemaApp.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _service;
        private readonly IUserService _userService;

        public BookingController(IBookingService service, IUserService userService)
        {
            _service = service;
            _userService = userService;
        }

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
        [AllowAnonymous]
        public IActionResult InitiateBooking([FromBody] BookingRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            // Extract user id from JWT and pass to service
            int? authUserId = null;
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(sub, out var userId))
                {
                    authUserId = userId;
                }
            }

            var booking = _service.InitiateBooking(request, authUserId);
            return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
        }

        [HttpPost("{id:int}/confirm")]
        [AllowAnonymous]
        public IActionResult ConfirmBooking(int id, [FromBody] BookingActionDTO? dto)
        {
            var booking = _service.GetById(id);

            if (Helpers.BookingAuthorization.IsAuthorized(User, _userService, booking, dto?.Email))
            {
                _service.ConfirmBooking(id);
                return NoContent();
            }

            return Forbid();
        }

        [HttpPost("{id:int}/cancel")]
        [AllowAnonymous]
        public IActionResult CancelBooking(int id, [FromBody] BookingActionDTO? dto)
        {
            var booking = _service.GetById(id);

            if (Helpers.BookingAuthorization.IsAuthorized(User, _userService, booking, dto?.Email))
            {
                _service.CancelBooking(id);
                return NoContent();
            }

            return Forbid();
        }

        [HttpGet("my-bookings")]
        [Authorize]
        public IActionResult GetMyBookings()
        {
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(sub, out var userId)) return Forbid();

            var bookings = _service.GetUserBookings(userId);
            return Ok(bookings);
        }
    }
}