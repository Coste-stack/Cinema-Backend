using CinemaApp.Model;
using CinemaApp.Repository;

namespace CinemaApp.Service;

public interface IRoomService
{
    List<Room> GetAll();
    Room? Get(int id);
    void Add(Room room);
    void Delete(int id);
    void Update(Room room);
}

public class RoomService : IRoomService
{
    private readonly IRoomRepository _repository;

    public RoomService(IRoomRepository repository) => _repository = repository;

    public List<Room> GetAll()
    {
        return _repository.GetAll().ToList();
    }

    public Room? Get(int id)
    {
        return _repository.GetById(id);
    }

    public void Add(Room room)
    {
        _repository.Add(room);
    }

    public void Delete(int id)
    {
        _repository.Delete(id);
    }

    public void Update(Room room)
    {
        _repository.Update(room);
    }
}