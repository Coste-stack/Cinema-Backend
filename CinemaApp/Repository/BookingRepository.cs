
using CinemaApp.Model;
using CinemaApp.Data;
using Microsoft.EntityFrameworkCore;
using CinemaApp.DTO;
using System.Linq;

namespace CinemaApp.Repository;

public interface IBookingRepository
{
    List<Booking> GetAll();
    Booking? GetById(int id);
    List<UserBookingDTO> GetByUserId(int userId);
    Booking Add(Booking booking);
    Booking Update(Booking booking);
    Booking UpdateBookingDiscountedPrice(int bookingId);
    
    decimal GetMoviePrice(Booking booking);
    decimal GetSeatPrice(int seatId);
    decimal GetPersonPercentDiscount(int personTypeId);
}

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context) => _context = context;

    public List<Booking> GetAll()
    {
        return _context.Bookings
            .AsNoTracking()
            .ToList();
    }

    public Booking? GetById(int id)
    {
        // Include tickets and dicount offers
        IQueryable<Booking> q = _context.Bookings
            .Include(b => b.Tickets)
                .ThenInclude(t => t.Seat)
            .Include(b => b.Tickets)
                .ThenInclude(t => t.PersonType)
            .Include(b => b.AppliedOffers);
        
        return q.FirstOrDefault(b => b.Id == id);
    }

    public List<UserBookingDTO> GetByUserId(int userId)
    {
        var bookings = _context.Bookings
            .Include(b => b.Tickets)
                .ThenInclude(t => t.Seat)
                    .ThenInclude(s => s.SeatType)
            .Include(b => b.Tickets)
                .ThenInclude(t => t.PersonType)
            .Include(b => b.Screening)
                .ThenInclude(s => s!.ProjectionType)
            .Include(b => b.Screening)
                .ThenInclude(s => s!.Movie)
            .Include(b => b.AppliedOffers)
            .Where(b => b.UserId == userId)
            .AsNoTracking()
            .ToList();

        return bookings.Select(b => new UserBookingDTO
        {
            BookingId = b.Id,
            Screening = new UserScreeningDTO
            {
                ScreeningId = b.Screening?.Id ?? 0,
                StartTime = b.Screening!.StartTime,
                EndTime = b.Screening!.EndTime ?? b.Screening!.StartTime,
                Language = b.Screening?.Language,
                ProjectionType = b.Screening!.ProjectionType.Name,
                Movie = new UserMovieDTO
                {
                    MovieId = b.Screening!.Movie.Id,
                    Title = b.Screening!.Movie.Title ?? string.Empty,
                    Genres = b.Screening!.Movie.Genres ?? new List<Genre>(),
                    Description = b.Screening!.Movie.Description,
                    ReleaseDate = b.Screening!.Movie.ReleaseDate?.ToString("yyyy-MM-dd"),
                    Rating = b.Screening!.Movie.Rating
                }
            },
            Tickets = (b.Tickets ?? Enumerable.Empty<Ticket>()).Select(t => new UserTicketDTO
            {
                TicketId = t.Id,
                TotalPrice = t.TotalPrice,
                PersonTypeName = t.PersonType?.Name ?? string.Empty,
                Seat = new UserSeatDTO
                {
                    SeatId = t.Seat?.Id ?? 0,
                    Row = t.Seat?.Row ?? "",
                    Number = t.Seat?.Number ?? 0,
                    SeatTypeName = t.Seat?.SeatType?.Name ?? string.Empty
                }
            }).ToList()
        }).ToList();
    }

    public Booking UpdateBookingDiscountedPrice(int bookingId)
    {
        // Get offers linked to booking
        var appliedOffers = _context.AppliedOffers
            .Where(a => a.BookingId == bookingId)
            .AsNoTracking()
            .ToList();

        // Sum discouts in offers
        decimal discountPrice = 0;
        foreach(var offer in appliedOffers) {
            discountPrice += offer.DiscountAmount;
        }

        // Get booking
        var booking = _context.Bookings
            .FirstOrDefault(b => b.Id == bookingId);
        if (booking == null)
        {
            throw new NotFoundException("No booking found to update discounted price");
        }
        
        // Update booking discounted price
        booking.DiscountedPrice = discountPrice;
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new ConflictException("No rows affected when updating discounted price.");
            return booking;
        }
        catch (DbUpdateException ex)
        {
            throw new ConflictException("Database update failed when updating discounted price.", ex);
        }
    }

    public Booking Add(Booking booking)
    {
        _context.Bookings.Add(booking);
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new ConflictException("No rows affected when adding a booking.");
            return booking;
        }
        catch (DbUpdateException ex)
        {
            throw new ConflictException("Database update failed when adding a booking.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Unexpected error when adding a booking.", ex);
        }
    }

    public Booking Update(Booking booking)
    {
        _context.Bookings.Update(booking);
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new ConflictException("No rows affected when updating a booking.");
            return booking;
        }
        catch (DbUpdateException ex)
        {
            throw new ConflictException("Database update failed when updating a booking.", ex);
        }
    }

    public decimal GetMoviePrice(Booking booking)
    {
        var price = _context.Bookings
            .Where(b => b.Id == booking.Id)
            .Select(b => b.Screening!.Movie.BasePrice)
            .FirstOrDefault();

        return price;
    }

    public decimal GetSeatPrice(int seatId)
    {
        var price = _context.Seats
            .Where(s => s.Id == seatId)
            .Select(s => s.SeatType.PriceAmountDiscount)
            .FirstOrDefault();

        return price;
    }
    
    public decimal GetPersonPercentDiscount(int personTypeId)
    {
        var price = _context.PersonTypes
            .Where(p => p.Id == personTypeId)
            .Select(p => p.PricePercentDiscount)
            .FirstOrDefault();
    
        return price;
    }
}