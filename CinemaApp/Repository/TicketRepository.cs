
using CinemaApp.Model;
using CinemaApp.Data;

namespace CinemaApp.Repository;

public interface ITicketRepository
{
    List<Ticket> GetAll();
    Ticket? GetById(int id);
    void Add(Ticket ticket);
    void Delete(int id);

    bool SeatExists(int seatId);
    bool IsSeatTaken(int seatId, int? screeningId);
}

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;

    public TicketRepository(AppDbContext context) => _context = context;

    public List<Ticket> GetAll() => _context.Tickets.ToList();

    public Ticket? GetById(int id) => _context.Tickets.Find(id);

    public void Add(Ticket ticket)
    {
        _context.Tickets.Add(ticket);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var ticket = _context.Tickets.Find(id);
        if (ticket == null) return;

        _context.Tickets.Remove(ticket);
        _context.SaveChanges();
    }

    public bool SeatExists(int seatId)
    {
        return _context.Seats.Any(s => s.Id == seatId);
    }

    public bool IsSeatTaken(int seatId, int? screeningId)
    {
        return _context.Tickets.Any(t => t.SeatId == seatId);
        // TODO: after adding adding booking table
        // return _context.Tickets
        //     .Join(_context.Bookings,
        //         t => t.BookingId,
        //         b => b.Id,
        //         (t,b) => new { t, b })
        //     .Any(x => x.t.SeatId == seatId &&
        //         x => x.b.ScreeningId == ScreeningId);
    }
}
