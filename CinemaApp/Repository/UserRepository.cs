
using CinemaApp.Model;
using CinemaApp.Data;

namespace CinemaApp.Repository;

public interface IUserRepository
{
    List<User> GetAll();
    User? GetById(int id);
    User? GetByEmail(string email);
    User Add(User user);
    void Update(User user);

    bool UserWithEmailExists(string email);
}

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public List<User> GetAll() => _context.Users.ToList();

    public User? GetById(int id) => _context.Users.Find(id);

    public User? GetByEmail(string email)
    {
        var lowered = email.ToLowerInvariant();
        return _context.Users
            .SingleOrDefault(u => u.Email.Equals(lowered, StringComparison.InvariantCultureIgnoreCase));
    }

    public User Add(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
        return user;
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
        _context.SaveChanges();
    }
    
    public bool UserWithEmailExists(string email)
    {
        var lowered = email.ToLowerInvariant();
        return _context.Users
            .Any(u => u.Email.Equals(lowered, StringComparison.InvariantCultureIgnoreCase));
    }
}
