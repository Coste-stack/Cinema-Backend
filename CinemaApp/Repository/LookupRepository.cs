
using CinemaApp.Data;
using CinemaApp.Model;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Repository;

public interface ILookupRepository<T> where T : LookupEntity
{
    IEnumerable<T> GetAll();
    T? GetById(int id);
    T? GetByName(string name);
    T Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}

public class LookupRepository<T> : ILookupRepository<T> where T : LookupEntity
{
    private readonly AppDbContext _context;

    public LookupRepository(AppDbContext context) => _context = context;

    public IEnumerable<T> GetAll()
    {
        return _context.Set<T>().ToList();
    }

    public T? GetById(int id)
    {
        return _context.Set<T>().Find(id);
    }

    public T? GetByName(string name)
    {
        return _context.Set<T>().FirstOrDefault(e => e.Name == name);
    }

    public T Add(T entity)
    {
        _context.Set<T>().Add(entity);
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new ConflictException("No rows affected when adding a enum.");
            return entity;
        }
        catch (DbUpdateException ex)
        {
            throw new ConflictException("Database update failed when adding a enum.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Unexpected error when adding a enum.", ex);
        }
    }

    public void Update(T entity)
    {
        _context.Set<T>().Update(entity);
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new ConflictException("No rows affected when updating a enum.");
        }
        catch (DbUpdateException ex)
        {
            throw new ConflictException("Database update failed when updating a enum.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Unexpected error when updating a enum.", ex);
        }
    }

    public void Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
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
}