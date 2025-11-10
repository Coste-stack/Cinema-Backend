using CinemaApp.DTO.Ticket;
using CinemaApp.Model;
using CinemaApp.Repository;

namespace CinemaApp.Service;

public interface IPriceCalculationService
{
    decimal CalculateTicketPrice(int screeningId, int seatId, int personTypeId);
    TicketBulkPriceResponseDTO CalculateBulkTicketPrices(int screeningId, List<TicketPriceRequestDTO> tickets);
}

public class PriceCalculationService : IPriceCalculationService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IScreeningRepository _screeningRepo;

    public PriceCalculationService(IBookingRepository bookingRepo, IScreeningRepository screeningRepo)
    {
        _bookingRepo = bookingRepo;
        _screeningRepo = screeningRepo;
    }

    public decimal CalculateTicketPrice(int screeningId, int seatId, int personTypeId)
    {
        // Validate screening exists
        var screening = _screeningRepo.GetById(screeningId);
        if (screening == null)
            throw new NotFoundException($"Screening with ID {screeningId} not found.");

        // Get base price from screening (or movie if screening doesn't have one)
        decimal totalPrice = screening.BasePrice > 0 ? screening.BasePrice : screening.Movie.BasePrice;

        // Add projection type price
        totalPrice += screening.ProjectionType.PriceAmountDiscount;

        // Add seat type price
        decimal seatPrice = _bookingRepo.GetSeatPrice(seatId);
        totalPrice += seatPrice;

        // Apply person type discount (percentage)
        decimal personPercentDiscount = _bookingRepo.GetPersonPercentDiscount(personTypeId);
        totalPrice *= (100 - personPercentDiscount) / 100;

        return Math.Round(totalPrice, 2);
    }

    public TicketBulkPriceResponseDTO CalculateBulkTicketPrices(int screeningId, List<TicketPriceRequestDTO> tickets)
    {
        var ticketPrices = new List<TicketPriceResponseDTO>();
        decimal totalPrice = 0;

        foreach (var ticket in tickets)
        {
            var price = CalculateTicketPrice(screeningId, ticket.SeatId, ticket.PersonTypeId);
            
            ticketPrices.Add(new TicketPriceResponseDTO
            {
                SeatId = ticket.SeatId,
                PersonTypeId = ticket.PersonTypeId,
                Price = price
            });
            
            totalPrice += price;
        }

        return new TicketBulkPriceResponseDTO
        {
            ScreeningId = screeningId,
            TicketPrices = ticketPrices,
            TotalPrice = Math.Round(totalPrice, 2)
        };
    }
}
