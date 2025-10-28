
using CinemaApp.Model;
using CinemaApp.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Repository;

public interface ISeatRepository
{
    List<Seat> GetByRoom(int roomId);
    List<Seat> AddRange(List<Seat> seats);
    void Delete(List<Seat> seats);
}

public class SeatRepository : ISeatRepository
{
    private readonly AppDbContext _context;

    public SeatRepository(AppDbContext context) => _context = context;

    public List<Seat> GetByRoom(int roomId) {
        return _context.Seats
            .Where(s => s.RoomId == roomId)
            .ToList();
    }

    public List<Seat> AddRange(List<Seat> seats)
    {
        _context.Seats.AddRange(seats);
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new InvalidOperationException("No rows affected when adding seats.");
            return seats;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Database update failed when adding seats.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unexpected error when adding seats.", ex);
        }
    }

    public void Delete(List<Seat> seats)
    {
        _context.Seats.RemoveRange(seats);
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new InvalidOperationException("No rows affected when deleting seats.");
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Database update failed when deleting seats.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unexpected error when deleting seats.", ex);
        }
    }
}
