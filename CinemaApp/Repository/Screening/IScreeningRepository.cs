using CinemaApp.Model;
using CinemaApp.Data;

namespace CinemaApp.Repository;

public interface IScreeningRepository
{
    List<Screening> GetAll();
    Screening? GetById(int id);
    void Add(Screening screening);
    void Update(Screening screening);
    void Delete(int id);
}