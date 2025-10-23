using CinemaApp.Model;
using CinemaApp.Repository;

namespace CinemaApp.Service;

public interface ISeatService
{
    List<Seat> GetAll();
    Seat? Get(int id);
    void AddRange(IEnumerable<Seat> seats);
    void Update(Seat seat);
    void Delete(int id);
}

public class SeatService : ISeatService
{
    private readonly ISeatRepository _repository;

    public SeatService(ISeatRepository repository) => _repository = repository;

    public List<Seat> GetAll()
    {
        return _repository.GetAll().ToList();
    }

    public Seat? Get(int id)
    {
        return _repository.GetById(id);
    }

    public void AddRange(IEnumerable<Seat> seats)
    {
        _repository.AddRange(seats);
    }

    public void Update(Seat seat)
    {
        _repository.Update(seat);
    }


    public void Delete(int id)
    {
        _repository.Delete(id);
    }
}