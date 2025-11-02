using CinemaApp.DTO;
using CinemaApp.Model;
using CinemaApp.Repository;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Service;

public interface IBookingService
{
    List<Booking> GetAll();
    Booking GetById(int id);
    Booking Create(BookingCreateDto dto);
}

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly ITicketRepository _ticketRepo;

    public BookingService(IBookingRepository bookingRepo, ITicketRepository ticketRepo)
    {
        _bookingRepo = bookingRepo;
        _ticketRepo = ticketRepo;
    }

    public List<Booking> GetAll()
    {
        return _bookingRepo.GetAll();
    }

    public Booking GetById(int id)
    {
        var booking = _bookingRepo.GetById(id);
        if (booking == null)
            throw new NotFoundException($"Booking with ID {id} not found.");
        return booking;
    }

    public Booking Create(BookingCreateDto dto)
    {
        if (dto == null)
            throw new BadRequestException("Booking data is required.");
            
        if (dto.Tickets == null || dto.Tickets.Count == 0)
            throw new BadRequestException("At least one ticket is required.");

        // Validate seats and availability for the screening
        foreach (TicketCreateDto t in dto.Tickets)
        {
            if (!_ticketRepo.SeatExists(t.SeatId))
                throw new BadRequestException($"Seat {t.SeatId} does not exist.");

            if (_ticketRepo.IsSeatTaken(t.SeatId, dto.ScreeningId))
                throw new ConflictException($"Seat {t.SeatId} is already taken for screening {dto.ScreeningId}.");
        }

        // Build aggregate
        Booking booking = new Booking
        {
            ScreeningId = dto.ScreeningId,
            BookingTime = dto.BookingTime,
            BookingStatus = BookingStatus.Confirmed
        };

        var moviePrice = _bookingRepo.GetMoviePrice(booking);

        // Create tickets and attach booking id
        foreach (TicketCreateDto t in dto.Tickets)
        {
            decimal totalPrice = moviePrice;
            decimal seatPrice = _bookingRepo.GetSeatPrice(t.SeatId);
            decimal personPercentDiscount = _bookingRepo.GetPersonPercentDiscount(t.PersonTypeId);
            totalPrice += seatPrice;
            totalPrice *= (100 - personPercentDiscount)/100;

            Ticket ticket = new Ticket
            {
                BookingId = booking.Id,
                SeatId = t.SeatId,
                PersonTypeId = t.PersonTypeId,
                TotalPrice = totalPrice
            };
            booking.Tickets.Add(ticket);
        }

        return _bookingRepo.Add(booking);
    }
}