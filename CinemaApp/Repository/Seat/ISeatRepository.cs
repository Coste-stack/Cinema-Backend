using CinemaApp.Model;
using CinemaApp.Data;

namespace CinemaApp.Repository;

public interface ISeatRepository
{
    List<Seat> GetAll();
    Seat? GetById(int id);
    void AddRange(IEnumerable<Seat> seats);
    void Update(Seat seat);
    void Delete(int id);
}