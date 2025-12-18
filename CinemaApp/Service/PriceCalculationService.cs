using CinemaApp.DTO.Ticket;
using CinemaApp.Model;
using CinemaApp.Repository;

namespace CinemaApp.Service;

public interface IPriceCalculationService
{
    decimal CalculateTicketPrice(int screeningId, int seatId, string personTypeName);
    BookingPriceResponseDTO CalculateBookingPrice(BookingPriceRequestDTO request);
    public decimal GetTotalBookingPrice(Booking booking);
}

public class PriceCalculationService(IBookingRepository bookingRepo, IScreeningRepository screeningRepo, ILookupService<PersonType> personTypeService) : IPriceCalculationService
{
    private readonly IBookingRepository _bookingRepo = bookingRepo;
    private readonly IScreeningRepository _screeningRepo = screeningRepo;
    private readonly ILookupService<PersonType> _personTypeService = personTypeService;

    public decimal CalculateTicketPrice(int screeningId, int seatId, string personTypeName)
    {
        if (screeningId <= 0 || seatId <= 0)
            throw new BadRequestException("All IDs must be positive integers.");

        if (string.IsNullOrEmpty(personTypeName))
            throw new BadRequestException("PersonType must be not be empty.");

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

        // Get person type by name
        var personType = _personTypeService.GetByName(personTypeName);
        if (personType == null)
            throw new NotFoundException($"PersonType with name '{personTypeName}' not found.");

        // Apply person type discount (percentage)
        decimal personPercentDiscount = _bookingRepo.GetPersonPercentDiscount(personType.Id);
        totalPrice *= (100 - personPercentDiscount) / 100;

        return Math.Round(totalPrice, 2);
    }

    public BookingPriceResponseDTO CalculateBookingPrice(BookingPriceRequestDTO request)
    {
        var ticketPrices = new List<TicketPriceResponseDTO>();
        decimal totalPrice = 0;

        foreach (var ticket in request.Tickets)
        {
            var price = CalculateTicketPrice(request.ScreeningId, ticket.SeatId, ticket.PersonTypeName);
            ticketPrices.Add(new TicketPriceResponseDTO
            {
                SeatId = ticket.SeatId,
                PersonTypeName = ticket.PersonTypeName,
                Price = price
            });
            totalPrice += price;
        }

        totalPrice = Math.Round(totalPrice, 2);

        return new BookingPriceResponseDTO
        {
            ScreeningId = request.ScreeningId,
            TicketPrices = ticketPrices,
            BasePrice = totalPrice,
            DiscountedPrice = totalPrice
        };
    }

    public decimal GetTotalBookingPrice(Booking booking)
    {
        var ticketRequests = booking.Tickets.Select(t => new TicketPriceRequestDTO
        {
            SeatId = t.SeatId,
            PersonTypeName = t.PersonType!.Name
        }).ToList();

        var request = new BookingPriceRequestDTO
        {
            ScreeningId = booking.ScreeningId,
            BookingTime = booking.BookingTime,
            Tickets = ticketRequests
        };

        BookingPriceResponseDTO response = CalculateBookingPrice(request);
        return Math.Round(response.BasePrice, 2);
    }
}
