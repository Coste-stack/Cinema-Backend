using CinemaApp.Model;
using CinemaApp.Repository;

namespace CinemaApp.Service;

public interface ICinemaService
{
    List<Cinema> GetAll();
    Cinema? GetById(int id);
    Cinema Add(Cinema cinema);
    void Update(int id, Cinema cinema);
}

public class CinemaService : ICinemaService
{
    private readonly ICinemaRepository _repository;

    public CinemaService(ICinemaRepository repository) => _repository = repository;

    public List<Cinema> GetAll()
    {
        return _repository.GetAll().ToList();
    }

    public Cinema? GetById(int id)
    {
        return _repository.GetById(id);
    }

    public Cinema Add(Cinema cinema)
    {
        if (cinema == null)
            throw new ArgumentException("Cinema data is required.");

        if (string.IsNullOrWhiteSpace(cinema.Name))
            throw new ArgumentException("Name cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(cinema.Address))
            throw new ArgumentException("Address cannot be null or empty.");

        return _repository.Add(cinema);
    }
    
    public void Update(int id, Cinema cinema)
    {
        var existing = _repository.GetById(id);
        if(existing == null) throw new KeyNotFoundException();

        if (!string.IsNullOrWhiteSpace(cinema.Name))
            existing.Name = cinema.Name.Trim();

        if (!string.IsNullOrWhiteSpace(cinema.Address))
            existing.Address = cinema.Address.Trim();

        if (!string.IsNullOrWhiteSpace(cinema.City))
            existing.City = cinema.City.Trim();

        _repository.Update(existing);
    }
}