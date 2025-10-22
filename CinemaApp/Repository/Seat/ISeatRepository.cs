using CinemaApp.Model;
using CinemaApp.Data;

namespace CinemaApp.Repository;

public interface ISeatRepository
{
    List<Seat> GetAll();
    Seat? GetById(int id);
    public void AddRange(IEnumerable<Seat> seats);
    void Delete(int id);
}