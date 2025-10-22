
using CinemaApp.Model;
using CinemaApp.Data;

namespace CinemaApp.Repository;

public class SeatRepository : ISeatRepository
{
    private readonly AppDbContext _context;

    public SeatRepository(AppDbContext context) => _context = context;

    public List<Seat> GetAll() => _context.Seats.ToList();

    public Seat? GetById(int id) => _context.Seats.Find(id);

    public void AddRange(IEnumerable<Seat> seats)
    {
        _context.Seats.AddRange(seats);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var seat = _context.Seats.Find(id);
        if (seat == null) return;

        _context.Seats.Remove(seat);
        _context.SaveChanges();
    }
}
