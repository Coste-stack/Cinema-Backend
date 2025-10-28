using CinemaApp.Model;
using CinemaApp.Repository;

namespace CinemaApp.Service;

public interface IRoomService
{
    List<Room> GetAll();
    Room? GetById(int id);
    Room Add(Room room);
    void Update(int id, Room room);
    void Delete(int id);
}

public class RoomService : IRoomService
{
    private readonly IRoomRepository _repository;

    public RoomService(IRoomRepository repository) => _repository = repository;

    public List<Room> GetAll()
    {
        return _repository.GetAll().ToList();
    }

    public Room? GetById(int id)
    {
        return _repository.GetById(id);
    }

    public Room Add(Room room)
    {
        if (!_repository.DoesCinemaExist(room.CinemaId))
            throw new KeyNotFoundException($"Cinema with ID {room.CinemaId} not found to link to room on create.");
            
        if (room == null)
            throw new ArgumentException("Room data is required.");

        if (string.IsNullOrWhiteSpace(room.Name))
            throw new ArgumentException("Name cannot be null or empty.");

        return _repository.Add(room);
    }

    public void Update(int id, Room room)
    {
        var existing = _repository.GetById(id);
        if (existing == null) 
            throw new KeyNotFoundException($"Room with ID {id} not found.");

        if (!string.IsNullOrWhiteSpace(room.Name))
            existing.Name = room.Name.Trim();

        if (room.CinemaId > 0) {
            if (!_repository.DoesCinemaExist(room.CinemaId)) {
                throw new KeyNotFoundException($"Cinema with ID {id} not found to link to room on update.");
            }
            existing.CinemaId = room.CinemaId;
        }
        _repository.Update(existing);
    }

    public void Delete(int id)
    {
        var existing = _repository.GetById(id);
        if (existing == null) 
            throw new KeyNotFoundException($"Room with ID {id} not found.");
        
        _repository.Delete(existing);
    }
}