using CinemaApp.Model;

namespace CinemaApp.Service;

public interface ISeatService
{
    List<Seat> GetAll();
    Seat? Get(int id);
    void AddRange(IEnumerable<Seat> seats);
    void Update(Seat seat);
    void Delete(int id);
}