
using CinemaApp.Model;
using CinemaApp.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Repository;

public interface IBookingRepository
{
    List<Booking> GetAll();
    Booking? GetById(int id);
    void Add(Booking booking);
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

    public void Add(Booking booking)
    {
        _context.Bookings.Add(booking);
        _context.SaveChanges();
    }
}