
using CinemaApp.Model;
using CinemaApp.Data;

namespace CinemaApp.Repository;

public class CinemaRepository : ICinemaRepository
{
    private readonly AppDbContext _context;

    public CinemaRepository(AppDbContext context) => _context = context;

    public List<Cinema> GetAll() => _context.Cinemas.ToList();

    public Cinema? GetById(int id) => _context.Cinemas.Find(id);

    public void Add(Cinema cinema)
    {
        _context.Cinemas.Add(cinema);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var cinema = _context.Cinemas.Find(id);
        if (cinema == null) return;

        _context.Cinemas.Remove(cinema);
        _context.SaveChanges();
    }

    public void Update(Cinema cinema)
    {
        Cinema? existingCinema = _context.Cinemas.Find(cinema.Id);
        if (existingCinema == null) return;

        if (!string.IsNullOrEmpty(cinema.Name))
            existingCinema.Name = cinema.Name;

        if (!string.IsNullOrEmpty(cinema.Address))
            existingCinema.Address = cinema.Address;

        if (!string.IsNullOrEmpty(cinema.City))
            existingCinema.City = cinema.City;

        _context.SaveChanges();
    }
}
