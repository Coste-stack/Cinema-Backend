using CinemaApp.Configuration;
using CinemaApp.DTO;
using CinemaApp.DTO.Ticket;
using CinemaApp.Model;
using CinemaApp.Repository;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Service;

public interface IBookingService
{
    List<Booking> GetAll();
    Booking GetById(int id);
    Booking Create(BookingCreateDto dto);
    Booking InitiateBooking(BookingRequestDTO request);
    void ConfirmBooking(int id);
    void CancelBooking(int id);
    List<Booking> GetUserBookings(int userId);
}

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly ITicketRepository _ticketRepo;
    private readonly IPriceCalculationService _priceService;

    public BookingService(
        IBookingRepository bookingRepo, 
        ITicketRepository ticketRepo,
        IPriceCalculationService priceService)
    {
        _bookingRepo = bookingRepo;
        _ticketRepo = ticketRepo;
        _priceService = priceService;
    }

    private static bool IsBookingExpired(Booking booking)
    {
        return booking.BookingStatus == BookingStatus.Pending &&
               booking.BookingTime.Add(BookingConfiguration.PendingBookingHoldDuration) < DateTime.UtcNow;
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

        // Create tickets and attach booking id
        foreach (TicketCreateDto t in dto.Tickets)
        {
            // Use the centralized price calculation service
            decimal totalPrice = _priceService.CalculateTicketPrice(
                dto.ScreeningId, 
                t.SeatId, 
                t.PersonTypeId);

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

    public Booking InitiateBooking(BookingRequestDTO request)
    {
        if (request == null)
            throw new BadRequestException("Booking data is required.");

        if (request.Tickets == null || request.Tickets.Count == 0)
            throw new BadRequestException("At least one ticket is required.");

        // Validate seats and availability for the screening
        foreach (TicketCreateDto t in request.Tickets)
        {
            if (!_ticketRepo.SeatExists(t.SeatId))
                throw new BadRequestException($"Seat {t.SeatId} does not exist.");

            if (_ticketRepo.IsSeatTaken(t.SeatId, request.ScreeningId))
                throw new ConflictException($"Seat {t.SeatId} is already taken for screening {request.ScreeningId}.");
        }

        // Build booking with Pending status to reserve seats
        Booking booking = new Booking
        {
            ScreeningId = request.ScreeningId,
            BookingTime = DateTime.UtcNow,
            BookingStatus = BookingStatus.Pending,
            UserId = request.UserId ?? 0
        };

        foreach (TicketCreateDto t in request.Tickets)
        {
            decimal totalPrice = _priceService.CalculateTicketPrice(
                request.ScreeningId,
                t.SeatId,
                t.PersonTypeId);

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

    public void ConfirmBooking(int id)
    {
        var existing = _bookingRepo.GetById(id);
        if (existing == null)
            throw new NotFoundException($"Booking with ID {id} not found.");

        if (existing.BookingStatus == BookingStatus.Confirmed)
            return;

        // Check if the pending booking has expired
        if (IsBookingExpired(existing))
        {
            existing.BookingStatus = BookingStatus.Cancelled;
            _bookingRepo.Update(existing);
            throw new ConflictException($"Booking {id} has expired and cannot be confirmed.");
        }

        existing.BookingStatus = BookingStatus.Confirmed;
        _bookingRepo.Update(existing);
    }

    public void CancelBooking(int id)
    {
        var existing = _bookingRepo.GetById(id);
        if (existing == null)
            throw new NotFoundException($"Booking with ID {id} not found.");

        if (existing.BookingStatus == BookingStatus.Cancelled)
            return;

        existing.BookingStatus = BookingStatus.Cancelled;
        _bookingRepo.Update(existing);
    }

    public List<Booking> GetUserBookings(int userId)
    {
        return _bookingRepo.GetByUserId(userId);
    }
}