
using CinemaApp.Data;
using CinemaApp.Model;

namespace CinemaApp.Repository;

public interface ILookupRepository<T> where T : LookupEntity
{
    IEnumerable<T> GetAll();
    T? GetById(int id);
    void Add(T entity);
    void Update(T entity);
    void Delete(int id);
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

    public void Add(T projectionType)
    {
        _context.Set<T>().Add(projectionType);
        _context.SaveChanges();
    }

    public void Update(T projectionType)
    {
        var existing = _context.Set<T>().Find(projectionType.Id);
        if (existing == null) return;

        existing.Name = projectionType.Name;
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var existing = _context.Set<T>().Find(id);
        if (existing == null) return;

        _context.Set<T>().Remove(existing);
        _context.SaveChanges();
    }
}