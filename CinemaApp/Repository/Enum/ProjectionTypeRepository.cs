
using CinemaApp.Data;
using CinemaApp.Model;

namespace CinemaApp.Repository;

public class ProjectionTypeRepository : IProjectionTypeRepository
{
    private readonly AppDbContext _context;

    public ProjectionTypeRepository(AppDbContext context) =>_context = context;

    public IEnumerable<ProjectionType> GetAll()
    {
        return _context.ProjectionTypes.ToList();
    }

    public ProjectionType? GetById(int id)
    {
        return _context.ProjectionTypes.Find(id);
    }

    public void Add(ProjectionType projectionType)
    {
        _context.ProjectionTypes.Add(projectionType);
        _context.SaveChanges();
    }

    public void Update(ProjectionType projectionType)
    {
        var existing = _context.ProjectionTypes.Find(projectionType.Id);
        if (existing == null) return;

        existing.Name = projectionType.Name;
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var existing = _context.ProjectionTypes.Find(id);
        if (existing == null) return;

        _context.ProjectionTypes.Remove(existing);
        _context.SaveChanges();
    }
}