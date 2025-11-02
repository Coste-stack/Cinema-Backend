
using CinemaApp.Model;
using CinemaApp.Data;
using Microsoft.EntityFrameworkCore;

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
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new ConflictException("No rows affected when adding a user.");
            return user;
        }
        catch (DbUpdateException ex)
        {
            throw new ConflictException("Database update failed when adding a user.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Unexpected error when adding a user.", ex);
        }
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new ConflictException("No rows affected when updating a user.");
        }
        catch (DbUpdateException ex)
        {
            throw new ConflictException("Database update failed when updating a user.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Unexpected error when updating a user.", ex);
        }
    }
    
    public bool UserWithEmailExists(string email)
    {
        var lowered = email.ToLowerInvariant();
        return _context.Users
            .Any(u => u.Email.Equals(lowered, StringComparison.InvariantCultureIgnoreCase));
    }
}
