using CinemaApp.Model;

namespace CinemaApp.Service;

public interface IProjectionTypeService
{
    List<ProjectionType> GetAll();
    ProjectionType? GetById(int id);
    void Create(ProjectionType projectionType);
    void Update(ProjectionType projectionType);
    void Delete(int id);
}
