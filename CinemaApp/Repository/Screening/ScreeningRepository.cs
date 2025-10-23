
using CinemaApp.Model;
using CinemaApp.Data;

namespace CinemaApp.Repository;

public class ScreeningRepository : IScreeningRepository
{
    private readonly AppDbContext _context;

    public ScreeningRepository(AppDbContext context) => _context = context;

    public List<Screening> GetAll() => _context.Screenings.ToList();

    public Screening? GetById(int id) => _context.Screenings.Find(id);

    public void Add(Screening screening)
    {
        Validate(screening);

        _context.Screenings.Add(screening);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var screening = _context.Screenings.Find(id);
        if (screening == null) return;

        _context.Screenings.Remove(screening);
        _context.SaveChanges();
    }

    public void Update(Screening screening)
    {
        Screening? existing = _context.Screenings.Find(screening.Id);
        if (existing == null) return;

        Validate(screening);

        if (screening.EndTime == null && existing.EndTime != null)
            screening.EndTime = existing.EndTime;

        if (screening.Language == null && existing.Language != null)
            screening.Language = existing.Language;

        _context.SaveChanges();
    }

    private void Validate(Screening screening)
    {
        if (screening.ProjectionTypeId <= 0)
            throw new ArgumentException("ProjectionTypeId must be specified.");
        if (screening.MovieId <= 0)
            throw new ArgumentException("MovieId must be specified.");
        if (screening.RoomId <= 0)
            throw new ArgumentException("RoomId must be specified.");

        if (screening.StartTime >= screening.EndTime)
            throw new ArgumentException("EndTime must be after StartTime.");
    }
}
