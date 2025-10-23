using CinemaApp.Model;
using CinemaApp.Repository;

namespace CinemaApp.Service;

public interface ITicketService
{
    List<Ticket> GetAll();
    Ticket? Get(int id);
    Ticket Add(Ticket ticket);
    void Delete(int id);
}

public class TicketService : ITicketService
{
    private readonly ITicketRepository _repository;

    public TicketService(ITicketRepository repository) => _repository = repository;

    public List<Ticket> GetAll()
    {
        return _repository.GetAll().ToList();
    }

    public Ticket? Get(int id)
    {
        return _repository.GetById(id);
    }

    public Ticket Add(Ticket ticket)
    {
        if (!_repository.SeatExists(ticket.SeatId))
            throw new ArgumentException("Seat does not exist.");

        // Check seat availability against screening id
        if (_repository.IsSeatTaken(ticket.SeatId, null))
            throw new ArgumentException("Seat is already taken.");

        _repository.Add(ticket);
        return ticket;
    }

    public void Delete(int id)
    {
        _repository.Delete(id);
    }
}