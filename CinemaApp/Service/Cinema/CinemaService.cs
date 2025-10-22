using CinemaApp.Model;
using CinemaApp.Repository;

namespace CinemaApp.Service;

public class CinemaService : ICinemaService
{
    private readonly ICinemaRepository _repository;

    public CinemaService(ICinemaRepository repository) => _repository = repository;

    public List<Cinema> GetAll()
    {
        return _repository.GetAll().ToList();
    }

    public Cinema? Get(int id)
    {
        return _repository.GetById(id);
    } 

    public void Add(Cinema cinema)
    {
        _repository.Add(cinema);
    }

    public void Delete(int id)
    {
        _repository.Delete(id);
    }

    public void Update(Cinema cinema)
    {
        _repository.Update(cinema);
    }
}