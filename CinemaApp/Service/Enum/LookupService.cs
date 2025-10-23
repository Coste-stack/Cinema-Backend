using CinemaApp.Model;
using CinemaApp.Repository;

namespace CinemaApp.Service;

public class LookupService<T> : ILookupService<T> where T : LookupEntity
{
    private readonly ILookupRepository<T> _repository;

    public LookupService(ILookupRepository<T> repository)
    {
        _repository = repository;
    }

    public List<T> GetAll() => _repository.GetAll().ToList();

    public T? GetById(int id) => _repository.GetById(id);

    public void Create(T projectionType) => _repository.Add(projectionType);

    public void Update(T projectionType) => _repository.Update(projectionType);

    public void Delete(int id) => _repository.Delete(id);
}
