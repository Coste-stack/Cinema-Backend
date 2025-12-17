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
    List<UserBookingDTO> GetUserBookings(int userId);
    Booking InitiateBooking(BookingRequestDTO request, int? authUserId = null);
    void ConfirmBooking(int id);
    void CancelBooking(int id);
    void UpdatePaymentInfo(int bookingId, string payUOrderId, decimal amount);
    void ConfirmPayment(string payUOrderId);
}

public class BookingService(
    IBookingRepository bookingRepo,
    ITicketRepository ticketRepo,
    IPriceCalculationService priceService,
    IUserService userService,
    ILookupService<PersonType> personTypeService,
    IOfferService offerService) : IBookingService
{
    private readonly IBookingRepository _bookingRepo = bookingRepo;
    private readonly ITicketRepository _ticketRepo = ticketRepo;
    private readonly IPriceCalculationService _priceService = priceService;
    private readonly IUserService _userService = userService;
    private readonly ILookupService<PersonType> _personTypeService = personTypeService;
    private readonly IOfferService _offerService = offerService;

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

    public List<UserBookingDTO> GetUserBookings(int userId)
    {
        return _bookingRepo.GetByUserId(userId);
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
            // Verify authenticated user exists
            var authUser = _userService.Get(authUserId.Value);
            if (authUser == null)
                throw new BadRequestException($"Authenticated user with ID {authUserId.Value} not found.");
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
                
                // Verify the user was created successfully
                if (resolvedUserId == 0)
                    throw new ConflictException("Failed to create guest user.");
            }
        }
        else
        {
            throw new BadRequestException("Either authentication or email is required to create a booking.");
        }

        // Final validation that we have a valid user ID
        if (resolvedUserId <= 0)
            throw new BadRequestException("Invalid user ID for booking.");

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

        var saved = _bookingRepo.Add(booking);

        try
        {
            // Build ticket price requests to evaluate offers for this booking
            var ticketRequests = request.Tickets.Select(t => new TicketPriceRequestDTO
            {
                SeatId = t.SeatId,
                PersonTypeName = t.PersonTypeName
            }).ToList();

            _offerService.ApplyOffersToBooking(saved.Id, saved.ScreeningId, ticketRequests);
        }
        catch
        {
            
        }

        return saved;
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

    public void UpdatePaymentInfo(int bookingId, string payUOrderId, decimal amount)
    {
        var booking = _bookingRepo.GetById(bookingId);
        if (booking == null)
            throw new NotFoundException($"Booking with ID {bookingId} not found.");

        booking.PayUOrderId = payUOrderId;
        booking.PaymentAmount = amount;
        _bookingRepo.Update(booking);
    }

    public void ConfirmPayment(string payUOrderId)
    {
        var bookings = _bookingRepo.GetAll();
        var booking = bookings.FirstOrDefault(b => b.PayUOrderId == payUOrderId);
        
        if (booking == null)
            throw new NotFoundException($"Booking with PayU Order ID {payUOrderId} not found.");

        if (booking.BookingStatus == BookingStatus.Confirmed)
            return; // Already confirmed

        if (IsBookingExpired(booking))
        {
            booking.BookingStatus = BookingStatus.Cancelled;
            _bookingRepo.Update(booking);
            throw new ConflictException($"Booking {booking.Id} has expired and cannot be confirmed.");
        }

        booking.BookingStatus = BookingStatus.Confirmed;
        booking.PaymentDate = DateTime.UtcNow;
        _bookingRepo.Update(booking);
    }
}