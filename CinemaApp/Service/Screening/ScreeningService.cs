using CinemaApp.Model;
using CinemaApp.Repository;

namespace CinemaApp.Service;

public class ScreeningService : IScreeningService
{
    private readonly IScreeningRepository _repository;

    public ScreeningService(IScreeningRepository repository) => _repository = repository;

    public List<Screening> GetAll()
    {
        return _repository.GetAll().ToList();
    }

    public Screening? Get(int id)
    {
        return _repository.GetById(id);
    } 

    public void Add(Screening screening)
    {
        CheckOverlappingScreenings(screening);
        
        _repository.Add(screening);
    }

    public void Delete(int id)
    {
        _repository.Delete(id);
    }

    public void Update(Screening screening)
    {
        CheckOverlappingScreenings(screening);

        _repository.Update(screening);
    }

    private void CheckOverlappingScreenings(Screening screening)
    {
        // Check for overlapping screenings in the same room
        var overlapping = _repository.GetAll()
            .Any(s => s.Id != screening.Id &&
                    s.RoomId == screening.RoomId &&
                    s.StartTime < screening.EndTime &&
                    s.EndTime > screening.StartTime);

        if (overlapping)
            throw new InvalidOperationException("Room is already booked for this time.");
    }
}