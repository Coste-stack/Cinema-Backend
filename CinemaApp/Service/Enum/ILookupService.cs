using CinemaApp.Model;

namespace CinemaApp.Service;

public interface ILookupService<T> where T : LookupEntity
{
    List<T> GetAll();
    T? GetById(int id);
    void Create(T entity);
    void Update(T entity);
    void Delete(int id);
}
