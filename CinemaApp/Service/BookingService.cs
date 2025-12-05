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
    Booking InitiateBooking(BookingRequestDTO request, int? authUserId = null);
    void ConfirmBooking(int id);
    void CancelBooking(int id);
    List<Booking> GetUserBookings(int userId);
}

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly ITicketRepository _ticketRepo;
    private readonly IPriceCalculationService _priceService;
    private readonly IUserService _userService;
    private readonly ILookupService<PersonType> _personTypeService;

    public BookingService(
        IBookingRepository bookingRepo, 
        ITicketRepository ticketRepo,
        IPriceCalculationService priceService,
        IUserService userService,
        ILookupService<PersonType> personTypeService)
    {
        _bookingRepo = bookingRepo;
        _ticketRepo = ticketRepo;
        _priceService = priceService;
        _userService = userService;
        _personTypeService = personTypeService;
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
            // Resolve PersonType by name
            var personType = _personTypeService.GetByName(t.PersonTypeName);
            if (personType == null)
                throw new BadRequestException($"PersonType '{t.PersonTypeName}' not found.");

            // Use the centralized price calculation service
            decimal totalPrice = _priceService.CalculateTicketPrice(
                dto.ScreeningId, 
                t.SeatId, 
                t.PersonTypeName);

            Ticket ticket = new Ticket
            {
                BookingId = booking.Id,
                SeatId = t.SeatId,
                PersonTypeId = personType.Id,
                TotalPrice = totalPrice
            };
            booking.Tickets.Add(ticket);
        }

        return _bookingRepo.Add(booking);
    }

    public Booking InitiateBooking(BookingRequestDTO request, int? authUserId = null)
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

        // Resolve user authenticated (jwt) or guest (email)
        int resolvedUserId = 0;
        if (authUserId != null && authUserId > 0)
        {
            resolvedUserId = authUserId.Value;
        }
        else if (!string.IsNullOrWhiteSpace(request.Email))
        {
            // Try to find existing user by email
            var existingUser = _userService.Get(request.Email);
            if (existingUser != null)
            {
                resolvedUserId = existingUser.Id;
            }
            else
            {
                // Create a guest user record (no password)
                var newUser = _userService.Add(new UserCreateDTO { Email = request.Email });
                resolvedUserId = newUser.Id;
            }
        }
        else
        {
            throw new BadRequestException("Either authentication or email is required to create a booking.");
        }

        // Build booking with Pending status to reserve seats
        Booking booking = new Booking
        {
            ScreeningId = request.ScreeningId,
            BookingTime = DateTime.UtcNow,
            BookingStatus = BookingStatus.Pending,
            UserId = resolvedUserId
        };

        foreach (TicketCreateDto t in request.Tickets)
        {
            // Resolve PersonType by name
            var personType = _personTypeService.GetByName(t.PersonTypeName);
            if (personType == null)
                throw new BadRequestException($"PersonType '{t.PersonTypeName}' not found.");

            decimal totalPrice = _priceService.CalculateTicketPrice(
                request.ScreeningId,
                t.SeatId,
                t.PersonTypeName);

            Ticket ticket = new Ticket
            {
                BookingId = booking.Id,
                SeatId = t.SeatId,
                PersonTypeId = personType.Id,
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