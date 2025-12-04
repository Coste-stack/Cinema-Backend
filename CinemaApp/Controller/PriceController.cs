using CinemaApp.DTO.Ticket;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controller;

[ApiController]
[Route("[controller]")]
public class PriceController : ControllerBase
{
    private readonly IPriceCalculationService _priceService;

    public PriceController(IPriceCalculationService priceService)
    {
        _priceService = priceService;
    }

    [HttpGet("calculate")]
    public ActionResult<object> CalculatePrice(
        [FromQuery] int screeningId,
        [FromQuery] int seatId,
        [FromQuery] string personTypeName)
    {
        decimal price = _priceService.CalculateTicketPrice(screeningId, seatId, personTypeName);
        return Ok(new { price });
    }

    [HttpPost("calculate-bulk")]
    public ActionResult<TicketBulkPriceResponseDTO> CalculateBulkPrice([FromBody] TicketBulkPriceRequestDTO request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = _priceService.CalculateBulkTicketPrices(request.ScreeningId, request.Tickets);
        return Ok(result);
    }
}
