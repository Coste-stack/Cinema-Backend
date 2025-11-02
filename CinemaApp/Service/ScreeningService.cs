using CinemaApp.Model;
using CinemaApp.Repository;

namespace CinemaApp.Service;

public interface IScreeningService
{
    List<Screening> GetAll();
    List<Screening> GetByMovie(int movieId);
    List<Screening> GetByRoom(int roomId);
    Screening GetById(int id);
    Screening Add(Screening screening);
    void Update(int id,Screening screening);
    void Delete(int id);
    void DeleteByMovie(int movieId);
    void DeleteByRoom(int roomId);
}

public class ScreeningService : IScreeningService
{
    private readonly IScreeningRepository _repository;
    private readonly IMovieRepository _movieRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly ILookupRepository<ProjectionType> _projectionTypeRepository;

    public ScreeningService(IScreeningRepository repository, IMovieRepository movieRepository, IRoomRepository roomRepository, ILookupRepository<ProjectionType> projectionTypeRepository)
    {
        _repository = repository;
        _movieRepository = movieRepository;
        _roomRepository = roomRepository;
        _projectionTypeRepository = projectionTypeRepository;
    }

    public List<Screening> GetAll()
    {
        return _repository.GetAll();
    }

    public List<Screening> GetByMovie(int movieId)
    {
        var movie = _movieRepository.GetById(movieId);
        if (movie == null)
            throw new NotFoundException($"Movie with ID {movieId} not found.");

        return _repository.GetByMovie(movieId);
    }
    
    public List<Screening> GetByRoom(int roomId)
    {
        var room = _roomRepository.GetById(roomId);
        if (room == null)
            throw new NotFoundException($"Room with ID {roomId} not found.");

        return _repository.GetByRoom(roomId);
    }

    public Screening GetById(int id)
    {
        var screening = _repository.GetById(id);
        if (screening == null)
            throw new NotFoundException($"Screening with ID {id} not found.");

        return screening;
    }

    public Screening Add(Screening screening)
    {
        if (screening.ProjectionTypeId <= 0)
            throw new BadRequestException("ProjectionTypeId must be specified.");
        if (screening.MovieId <= 0)
            throw new BadRequestException("MovieId must be specified.");
        if (screening.RoomId <= 0)
            throw new BadRequestException("RoomId must be specified.");

        var projectionType = _projectionTypeRepository.GetById(screening.ProjectionTypeId);
        if (projectionType == null)
            throw new NotFoundException($"ProjectionType with ID {screening.ProjectionTypeId} not found.");

        var movie = _movieRepository.GetById(screening.MovieId);
        if (movie == null)
            throw new NotFoundException($"Movie with ID {screening.MovieId} not found.");
            
        var room = _roomRepository.GetById(screening.RoomId);
        if (room == null)
            throw new NotFoundException($"Room with ID {screening.RoomId} not found.");

        // Add movie duration to screening datetime
        screening.EndTime = screening.StartTime.AddMinutes(movie.Duration);

        if (screening.StartTime >= screening.EndTime)
            throw new BadRequestException("EndTime must be after StartTime.");

        CheckOverlappingScreenings(screening);

        return _repository.Add(screening);
    }

    public void Update(int id, Screening screening)
    {
        if (id != screening.Id)
        throw new BadRequestException($"ID {id} and ID {screening.Id} mismatch in request objects");

        var existing = _repository.GetById(id);
        if (existing == null)
            throw new NotFoundException($"Screening with ID {id} not found.");

        // Determine candidate values: if caller sent "0" for ids we keep existing; otherwise use provided values.
        var newRoomId = screening.RoomId > 0 ? screening.RoomId : existing.RoomId;
        var newMovieId = screening.MovieId > 0 ? screening.MovieId : existing.MovieId;
        var newProjectionTypeId = screening.ProjectionTypeId > 0 ? screening.ProjectionTypeId : existing.ProjectionTypeId;

        // Validate lookups for changed ids
        var projectionType = _projectionTypeRepository.GetById(newProjectionTypeId);
        if (projectionType == null)
            throw new NotFoundException($"ProjectionType with ID {newProjectionTypeId} not found.");

        var movie = _movieRepository.GetById(newMovieId);
        if (movie == null)
            throw new NotFoundException($"Movie with ID {newMovieId} not found.");

        var room = _roomRepository.GetById(newRoomId);
        if (room == null)
            throw new NotFoundException($"Room with ID {newRoomId} not found.");

        // Decide StartTime: prefer provided non-default value; otherwise keep existing
        var newStartTime = screening.StartTime != default(DateTime) ? screening.StartTime : existing.StartTime;

        // Compute expected EndTime from movie duration
        var expectedEndTime = newStartTime.AddMinutes(movie.Duration);
        DateTime? newEndTime;
        if (screening.EndTime == null)
        {
            newEndTime = existing.EndTime;
        }
        else
        {
            newEndTime = expectedEndTime;
        }

        if (newEndTime == null)
            throw new BadRequestException("Couldnt get EndTime.");

        if (newStartTime >= newEndTime.Value)
            throw new BadRequestException("EndTime must be after StartTime.");

        // Apply changes to the existing entity
        existing.StartTime = newStartTime;
        existing.EndTime = newEndTime;
        existing.RoomId = newRoomId;
        existing.MovieId = newMovieId;
        existing.ProjectionTypeId = newProjectionTypeId;
        existing.Language = screening.Language ?? existing.Language;

        CheckOverlappingScreenings(existing);

        _repository.Update(existing);
    }

    public void Delete(int id)
    {
        var existing = _repository.GetById(id);
        if (existing == null)
            throw new NotFoundException($"Screening with ID {id} not found.");

        _repository.Delete(existing);
    }

    public void DeleteByMovie(int movieId)
    {
        var movie = _movieRepository.GetById(movieId);
        if (movie == null)
            throw new NotFoundException($"Movie with ID {movieId} not found.");

        var screenings = _repository.GetByMovie(movieId);
        if (screenings.Count == 0)
            throw new NotFoundException($"No screenings found to delete for movie with ID {movieId}.");

        _repository.DeleteRange(screenings);
    }
    
    public void DeleteByRoom(int roomId)
    {
        var movie = _roomRepository.GetById(roomId);
        if (movie == null)
            throw new NotFoundException($"Room with ID {roomId} not found.");

        var screenings = _repository.GetByRoom(roomId);
        if (screenings.Count == 0)
            throw new NotFoundException($"No screenings found to delete for room with ID {roomId}.");

        _repository.DeleteRange(screenings);
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
            throw new ConflictException("Room is already booked for this time.");
    }
}