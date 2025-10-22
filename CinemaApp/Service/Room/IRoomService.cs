using CinemaApp.Model;

namespace CinemaApp.Service;

public interface IRoomService
{
    List<Room> GetAll();
    Room? Get(int id);
    void Add(Room room);
    void Delete(int id);
    void Update(Room room);
}