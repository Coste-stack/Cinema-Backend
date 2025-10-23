using CinemaApp.Model;
using CinemaApp.Repository;

namespace CinemaApp.Service;

public class ProjectionTypeService : IProjectionTypeService
{
    private readonly IProjectionTypeRepository _repository;

    public ProjectionTypeService(IProjectionTypeRepository repository)
    {
        _repository = repository;
    }

    public List<ProjectionType> GetAll() => _repository.GetAll().ToList();

    public ProjectionType? GetById(int id) => _repository.GetById(id);

    public void Create(ProjectionType projectionType) => _repository.Add(projectionType);

    public void Update(ProjectionType projectionType) => _repository.Update(projectionType);

    public void Delete(int id) => _repository.Delete(id);
}
