using CinemaApp.Model;

namespace CinemaApp.Repository;

public interface IProjectionTypeRepository
{
    IEnumerable<ProjectionType> GetAll();
    ProjectionType? GetById(int id);
    void Add(ProjectionType projectionType);
    void Update(ProjectionType projectionType);
    void Delete(int id);
}