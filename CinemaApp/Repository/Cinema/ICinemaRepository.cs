using CinemaApp.Model;
using CinemaApp.Data;

namespace CinemaApp.Repository;

public interface ICinemaRepository
{
    List<Cinema> GetAll();
    Cinema? GetById(int id);
    void Add(Cinema cinema);
    void Update(Cinema cinema);
    void Delete(int id);
}