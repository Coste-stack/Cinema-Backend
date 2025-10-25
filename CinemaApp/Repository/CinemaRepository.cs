
using CinemaApp.Model;
using CinemaApp.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Repository;

public interface ICinemaRepository
{
    List<Cinema> GetAll();
    Cinema? GetById(int id);
    Cinema Add(Cinema cinema);
    void Update(Cinema cinema);
}

public class CinemaRepository : ICinemaRepository
{
    private readonly AppDbContext _context;

    public CinemaRepository(AppDbContext context) => _context = context;

    public List<Cinema> GetAll() => _context.Cinemas.ToList();

    public Cinema? GetById(int id) => _context.Cinemas.Find(id);

    public Cinema Add(Cinema cinema)
    {
        _context.Cinemas.Add(cinema);
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new InvalidOperationException("No rows affected when adding a cinema.");
            return cinema;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Database update failed when adding a cinema.", ex);
        }
    }

    public void Update(Cinema cinema)
    {
        _context.Cinemas.Update(cinema);
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new InvalidOperationException("No rows affected when updating a cinema.");
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Database update failed when updating a cinema.", ex);
        }
    }
}
