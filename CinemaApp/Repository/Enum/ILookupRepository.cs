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