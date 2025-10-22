using CinemaApp.Model;

namespace CinemaApp.Service;

public interface IScreeningService
{
    List<Screening> GetAll();
    Screening? Get(int id);
    void Add(Screening screening);
    void Delete(int id);
    void Update(Screening screening);
}