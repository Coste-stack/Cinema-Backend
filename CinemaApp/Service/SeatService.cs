using CinemaApp.Model;
using CinemaApp.Repository;

namespace CinemaApp.Service;

public interface ISeatService
{
    List<Seat> GetByRoom(int roomId);
    List<Seat> AddRange(int roomId, int rows, int seatsPerRow, int seatTypeId);
    void Delete(int roomId);
}

public class SeatService : ISeatService
{
    private readonly ISeatRepository _repository;
    private readonly IRoomRepository _roomRepository;

    public SeatService(ISeatRepository repository, IRoomRepository roomRepository)
    {
        _repository = repository;
        _roomRepository = roomRepository;
    }

    public List<Seat> GetByRoom(int roomId)
    {
        var room = _roomRepository.GetById(roomId);
        if (room == null)
            throw new KeyNotFoundException($"Room with ID {roomId} not found.");

        return _repository.GetByRoom(roomId);
    }

    public List<Seat> AddRange(int roomId, int rows, int seatsPerRow, int seatTypeId)
    {
        var existingRoom = _roomRepository.GetById(roomId);
        if (existingRoom == null) 
            throw new KeyNotFoundException($"Room with ID {roomId} not found.");

        var seats= new List<Seat>();
        for (char row = 'A'; row < 'A' + rows; row++)
        {
            for (int number = 1; number <= seatsPerRow; number++)
            {
                seats.Add(new Seat
                {
                    RoomId = roomId,
                    Row = row.ToString(),
                    Number = number,
                    SeatTypeId = seatTypeId
                });
            }
        }

        return _repository.AddRange(seats);
    }


    public void Delete(int roomId)
    {
        var existingRoom = _roomRepository.GetById(roomId);
        if (existingRoom == null) 
            throw new KeyNotFoundException($"Room with ID {roomId} not found.");

        var seats = _repository.GetByRoom(roomId);
        if (seats.Count == 0) 
            throw new KeyNotFoundException("No seats found for this room.");
        
        _repository.Delete(seats);
    }
}