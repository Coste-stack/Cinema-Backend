using CinemaApp.DTO.Ticket;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controller;

[ApiController]
[Route("api/[controller]")]
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
        [FromQuery] int personTypeId)
    {
        if (screeningId <= 0 || seatId <= 0 || personTypeId <= 0)
            return BadRequest("All IDs must be positive integers.");

        var price = _priceService.CalculateTicketPrice(screeningId, seatId, personTypeId);
        return Ok(new { 
            screeningId, 
            seatId, 
            personTypeId, 
            price 
        });
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
