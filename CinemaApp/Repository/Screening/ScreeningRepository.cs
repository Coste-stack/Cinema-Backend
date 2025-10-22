
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
        Validate(screening);

        _context.SaveChanges();
    }

    private void Validate(Screening screening)
    {
        Screening? existingScreening = _context.Screenings.Find(screening.Id);
        if (existingScreening == null) return;

        if (screening.EndTime == null && existingScreening.EndTime != null)
            screening.EndTime = existingScreening.EndTime;

        if (screening.Language == null && existingScreening.Language != null)
            screening.Language = existingScreening.Language;

        if (screening.StartTime >= screening.EndTime)
            throw new ArgumentException("EndTime must be after StartTime.");

        if (screening.Price <= 0)
            throw new ArgumentException("Price must be greater than zero.");
    }
}
