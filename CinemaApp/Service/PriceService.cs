using CinemaApp.DTO.Ticket;
using CinemaApp.Model;
using CinemaApp.Repository;

namespace CinemaApp.Service;

public interface IPriceService
{
    decimal GetTicketPrice(int screeningId, int seatId, string personTypeName);
    BookingPriceResponseDTO GetBookingPrices(int bookingId, int screeningId, List<TicketPriceRequestDTO> tickets);
}

public class PriceService(IPriceCalculationService priceCalcService, IOfferService offerService) : IPriceService
{
    private readonly IPriceCalculationService _priceCalcService = priceCalcService;
    private readonly IOfferService _offerService = offerService;

    public decimal GetTicketPrice(int screeningId, int seatId, string personTypeName)
    {
        return _priceCalcService.CalculateTicketPrice(screeningId, seatId, personTypeName);
    }

    public BookingPriceResponseDTO GetBookingPrices(int bookingId, int screeningId, List<TicketPriceRequestDTO> tickets)
    {
        var request = new BookingPriceRequestDTO
        {
            ScreeningId = screeningId,
            BookingTime = DateTime.UtcNow,
            Tickets = tickets
        };

        BookingPriceResponseDTO response = _priceCalcService.CalculateBookingPrice(request);

        // Evaluate offers
        var appliedOffers = _offerService.GetAppliedOffers(screeningId, tickets, bookingId);
        var discountAmount = _offerService.SumAppliedOffers(appliedOffers);

        response.TotalPrice -= discountAmount;
        if (response.TotalPrice < 0) response.TotalPrice = 0m;

        // Map applied offers for response
        response.AppliedOffers = appliedOffers.Select(a => new AppliedOfferResponseDTO
        {
            OfferId = a.OfferId,
            OfferName = a.Offer?.Name,
            DiscountAmount = a.DiscountAmount
        }).ToList();

        return response;
    }
}
