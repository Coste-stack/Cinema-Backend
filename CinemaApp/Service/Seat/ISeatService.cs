using CinemaApp.Model;

namespace CinemaApp.Service;

public interface ISeatService
{
    List<Seat> GetAll();
    Seat? Get(int id);
    public void AddRange(IEnumerable<Seat> seats);
    void Delete(int id);
}