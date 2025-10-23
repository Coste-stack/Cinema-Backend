
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
        foreach (Seat seat in seats)
        {
            Validate(seat);
        }

        _context.Seats.AddRange(seats);
        _context.SaveChanges();
    }

    public void Update(Seat seat)
    {
        Seat? existing = _context.Seats.Find(seat.Id);
        if (existing == null)
            throw new KeyNotFoundException($"Seat with ID {seat.Id} not found.");

        Validate(seat);

        existing.Status = seat.Status;

        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        Seat? seat = _context.Seats.Find(id);
        if (seat == null)
            throw new KeyNotFoundException($"Seat with ID {id} not found.");

        _context.Seats.Remove(seat);
        _context.SaveChanges();
    }
    
    private void Validate(Seat seat)
    {
        if (!Enum.IsDefined(seat.Status))
            throw new ArgumentException($"Invalid seat status: {seat.Status}");
        
        if (seat.SeatTypeId <= 0)
            throw new ArgumentException("SeatTypeId must be specified.");
    }
}
