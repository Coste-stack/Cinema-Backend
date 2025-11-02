
using CinemaApp.Model;
using CinemaApp.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Repository;

public interface IRoomRepository
{
    List<Room> GetAll();
    Room? GetById(int id);
    Room Add(Room room);
    void Update(Room room);
    void Delete(Room room);

    public bool DoesCinemaExist(int cinemaId);
}

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _context;

    public RoomRepository(AppDbContext context) => _context = context;

    public List<Room> GetAll() => _context.Rooms.ToList();

    public Room? GetById(int id) => _context.Rooms.Find(id);

    public Room Add(Room room)
    {
        _context.Rooms.Add(room);
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new ConflictException("No rows affected when adding a room.");
            return room;
        }
        catch (DbUpdateException ex)
        {
            throw new ConflictException("Database update failed when adding a room.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Unexpected error when adding a room.", ex);
        }
    }

    public void Update(Room room)
    {
        _context.Rooms.Update(room);
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new ConflictException("No rows affected when updating a room.");
        }
        catch (DbUpdateException ex)
        {
            throw new ConflictException("Database update failed when updating a room.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Unexpected error when updating a room.", ex);
        }
    }

    public void Delete(Room room)
    {
        _context.Rooms.Remove(room);
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new ConflictException("No rows affected when deleting a room.");
        }
        catch (DbUpdateException ex)
        {
            throw new ConflictException("Database update failed when deleting a room.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Unexpected error when deleting a room.", ex);
        }
    }

    public bool DoesCinemaExist(int cinemaId)
    {
        var cinema = _context.Cinemas.FirstOrDefault(c => c.Id == cinemaId);
        if (cinema != null) return true;
        return false;
    }
}
