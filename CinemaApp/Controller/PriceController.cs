using CinemaApp.DTO.Ticket;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controller;

[ApiController]
[Route("[controller]")]
public class PriceController : ControllerBase
{
    private readonly IPriceService _priceService;

    public PriceController(IPriceService priceService)
    {
        _priceService = priceService;
    }

    [HttpGet("ticket")]
    public ActionResult<object> CalculatePrice(
        [FromQuery] int screeningId,
        [FromQuery] int seatId,
        [FromQuery] string personTypeName)
    {
        decimal price = _priceService.GetTicketPrice(screeningId, seatId, personTypeName);
        return Ok(new { price });
    }

    [HttpPost("booking")]
    public ActionResult<BookingPriceResponseDTO> CalculateBulkPrice([FromBody] BookingPriceRequestDTO request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = _priceService.GetBookingPrices(request.BookingId, request.ScreeningId, request.Tickets);
        return Ok(result);
    }
}
