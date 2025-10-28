
using CinemaApp.Model;
using CinemaApp.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Repository;

public interface IScreeningRepository
{
    List<Screening> GetAll();
    List<Screening> GetByMovie(int movieId);
    List<Screening> GetByRoom(int roomId);
    Screening? GetById(int id);
    Screening Add(Screening screening);
    void Update(Screening screening);
    void Delete(Screening screening);
    void DeleteRange(List<Screening> screenings);
}

public class ScreeningRepository : IScreeningRepository
{
    private readonly AppDbContext _context;

    public ScreeningRepository(AppDbContext context) => _context = context;

    public List<Screening> GetAll() => _context.Screenings.ToList();

    public List<Screening> GetByMovie(int movieId) => _context.Screenings.Where(s => s.MovieId == movieId).ToList();

    public List<Screening> GetByRoom(int roomId) => _context.Screenings.Where(s => s.RoomId == roomId).ToList();

    public Screening? GetById(int id) => _context.Screenings.Find(id);

    public Screening Add(Screening screening)
    {
        _context.Screenings.Add(screening);
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new InvalidOperationException("No rows affected when adding a screening.");
            return screening;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Database update failed when adding a screening.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unexpected error when adding a screening.", ex);
        }
        
    }

    public void Update(Screening screening)
    {
        _context.Screenings.Update(screening);
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new InvalidOperationException("No rows affected when updating a screening.");
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Database update failed when updating a screening.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unexpected error when updating a screening.", ex);
        }
    }

    public void Delete(Screening screening)
    {
        _context.Screenings.Remove(screening);
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new InvalidOperationException("No rows affected when deleting a screening.");
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Database update failed when deleting a screening.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unexpected error when deleting a screening.", ex);
        }
    }

    public void DeleteRange(List<Screening> screenings)
    {
        _context.Screenings.RemoveRange(screenings);
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new InvalidOperationException("No rows affected when deleting screenings.");
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Database update failed when deleting screenings.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unexpected error when deleting screenings.", ex);
        }
    }
}
