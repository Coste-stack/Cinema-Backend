using CinemaApp.DTO;
using CinemaApp.Model;
using CinemaApp.Repository;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Service;

public interface IBookingService
{
    List<Booking> GetAll();
    Booking? Get(int id);
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

    public List<Booking> GetAll() => _bookingRepo.GetAll();

    public Booking? Get(int id) => _bookingRepo.GetById(id);

    public Booking Create(BookingCreateDto dto)
    {
        if (dto == null) throw new ArgumentException("Booking data is required.");
        if (dto.Tickets == null || dto.Tickets.Count == 0)
            throw new ArgumentException("At least one ticket is required.");

        // Validate seats and availability for the screening
        foreach (TicketCreateDto t in dto.Tickets)
        {
            if (!_ticketRepo.SeatExists(t.SeatId))
                throw new ArgumentException($"Seat {t.SeatId} does not exist.");

            if (_ticketRepo.IsSeatTaken(t.SeatId, dto.ScreeningId))
                throw new InvalidOperationException($"Seat {t.SeatId} is already taken for screening {dto.ScreeningId}.");
        }

        // Build aggregate
        Booking booking = new Booking
        {
            ScreeningId = dto.ScreeningId,
            BookingTime = dto.BookingTime,
            BookingStatus = BookingStatus.Confirmed
        };

        // Create tickets and attach booking id
        foreach (TicketCreateDto t in dto.Tickets)
        {
            Ticket ticket = new Ticket
            {
                BookingId = booking.Id,
                SeatId = t.SeatId,
                PersonTypeId = t.PersonTypeId
            };
            booking.Tickets.Add(ticket);
        }

        // Persist changes
        try
        {
            _bookingRepo.Add(booking);
        }
        catch (DbUpdateException dbEx)
        {
            throw new InvalidOperationException("One or more seats are already taken for that screening.", dbEx);
        }

        // Return booking with tickets
        return _bookingRepo.GetById(booking.Id) ?? booking;
    }
}