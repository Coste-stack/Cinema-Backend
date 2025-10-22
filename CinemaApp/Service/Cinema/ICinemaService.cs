using CinemaApp.Model;

namespace CinemaApp.Service;

public interface ICinemaService
{
    List<Cinema> GetAll();
    Cinema? Get(int id);
    void Add(Cinema cinema);
    void Delete(int id);
    void Update(Cinema cinema);
}