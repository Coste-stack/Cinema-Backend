using CinemaApp.Model;
using CinemaApp.Data;

namespace CinemaApp.Repository;

public interface IRoomRepository
{
    List<Room> GetAll();
    Room? GetById(int id);
    void Add(Room room);
    void Update(Room room);
    void Delete(int id);
}