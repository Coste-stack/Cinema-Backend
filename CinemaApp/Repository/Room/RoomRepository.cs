
using CinemaApp.Model;
using CinemaApp.Data;

namespace CinemaApp.Repository;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _context;

    public RoomRepository(AppDbContext context) => _context = context;

    public List<Room> GetAll() => _context.Rooms.ToList();

    public Room? GetById(int id) => _context.Rooms.Find(id);

    public void Add(Room room)
    {
        _context.Rooms.Add(room);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var room = _context.Rooms.Find(id);
        if (room == null) return;

        _context.Rooms.Remove(room);
        _context.SaveChanges();
    }

    public void Update(Room room)
    {
        Room? existingRoom = _context.Rooms.Find(room.Id);
        if (existingRoom == null) return;

        if (!string.IsNullOrEmpty(room.Name))
            existingRoom.Name = room.Name;

        existingRoom.CinemaId = room.CinemaId;

        _context.SaveChanges();
    }
}
