
using CinemaApp.Model;
using CinemaApp.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Repository;

public interface IBookingRepository
{
    List<Booking> GetAll();
    Booking? GetById(int id);
    List<Booking> GetByUserId(int userId);
    Booking Add(Booking booking);
    Booking Update(Booking booking);
    
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
        // Include tickets
        return _context.Bookings
            .Include(b => b.Tickets)
                .ThenInclude(t => t.Seat)
            .Include(b => b.Tickets)
                .ThenInclude(t => t.PersonType)
            .FirstOrDefault(b => b.Id == id);
    }

    public List<Booking> GetByUserId(int userId)
    {
        return _context.Bookings
            .Include(b => b.Tickets)
                .ThenInclude(t => t.Seat)
            .Include(b => b.Tickets)
                .ThenInclude(t => t.PersonType)
            .Where(b => b.UserId == userId)
            .ToList();
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