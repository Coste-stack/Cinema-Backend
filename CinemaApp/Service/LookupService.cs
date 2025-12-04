using CinemaApp.Model;
using CinemaApp.Repository;

namespace CinemaApp.Service;

public interface ILookupService<T> where T : LookupEntity
{
    List<T> GetAll();
    T? GetById(int id);
    T? GetByName(string name);
    T Create(T entity);
    void Update(int id, T entity);
    void Delete(int id);
}

public class LookupService<T> : ILookupService<T> where T : LookupEntity
{
    private readonly ILookupRepository<T> _repository;

    public LookupService(ILookupRepository<T> repository)
    {
        _repository = repository;
    }

    public List<T> GetAll() => _repository.GetAll().ToList();

    public T? GetById(int id) => _repository.GetById(id);

    public T? GetByName(string name) => _repository.GetByName(name);

    public T Create(T projectionType)
    {
        return _repository.Add(projectionType);
    }

    public void Update(int id, T projectionType)
    {
        if (id != projectionType.Id) throw new BadRequestException("Enum ID mismatch in update.");
            
        var existing = _repository.GetById(id);
        if (existing == null) throw new NotFoundException("Enum not found to update.");

        existing.Name = projectionType.Name;
        _repository.Update(projectionType);
    } 

    public void Delete(int id)
    {
        var existing = _repository.GetById(id);
        if (existing == null) throw new NotFoundException("Enum not found to delete.");

        _repository.Delete(existing);
    } 
}
