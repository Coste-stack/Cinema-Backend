
using CinemaApp.Model;
using CinemaApp.Data;

namespace CinemaApp.Repository;

public interface ITicketRepository
{
    bool SeatExists(int seatId);
    bool IsSeatTaken(int seatId, int screeningId);
}

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;

    public TicketRepository(AppDbContext context) => _context = context;

    public bool SeatExists(int seatId)
    {
        return _context.Seats
            .Any(s => s.Id == seatId);
    }

    public bool IsSeatTaken(int seatId, int screeningId)
    {
        return _context.Tickets
            .Any(t => t.SeatId == seatId &&
                t.ScreeningId == screeningId);
    }
}
