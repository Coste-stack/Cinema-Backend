
using CinemaApp.Model;
using CinemaApp.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Repository;

public interface IBookingRepository
{
    List<Booking> GetAll();
    Booking? GetById(int id);
    Booking Add(Booking booking);
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
        // include tickets if you want them returned with booking
        return _context.Bookings
            .Include(b => b.Screening)
            .FirstOrDefault(b => b.Id == id);
    }

    public Booking Add(Booking booking)
    {
        _context.Bookings.Add(booking);
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new InvalidOperationException("No rows affected when adding a booking.");
            return booking;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Database update failed when adding a booking.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unexpected error when adding a booking.", ex);
        }
    }
}