using CinemaApp.Model;
using CinemaApp.Repository;
using Microsoft.AspNetCore.Identity;

namespace CinemaApp.Service;

public interface IUserService
{
    List<User> GetAll();
    User? Get(int id);
    User? Get(string email);
    User Add(UserCreateDTO dto);
    void Update(int id, UserCreateDTO dto);
}

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(IUserRepository repository, IPasswordHasher<User> passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
    }

    public List<User> GetAll()
    {
        return _repository.GetAll().ToList();

    }

    public User? Get(int id)
    {
        return _repository.GetById(id);
    }

    public User? Get(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email not valid.");
        
        return _repository.GetByEmail(email);
    }

    public User Add(UserCreateDTO dto)
    {
        if (dto == null) 
            throw new ArgumentException("User data is required.");

        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentException("Email is required.");

        if (_repository.UserWithEmailExists(dto.Email))
            throw new ArgumentException("Email already used.");

        var user = new User{
            Email = dto.Email
        };
        
        if (!string.IsNullOrEmpty(dto.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
            user.UserType = UserType.Registered;
        }

        return _repository.Add(user);
    }

    public void Update(int id, UserCreateDTO dto)
    {
        var existingUser = _repository.GetById(id);
        if (existingUser == null) throw new KeyNotFoundException("User not found.");

        // Replace email
        if (!string.IsNullOrWhiteSpace(dto.Email) && !string.Equals(existingUser.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (_repository.UserWithEmailExists(dto.Email))
                throw new ArgumentException("Email already used.");

            existingUser.Email = dto.Email;
        }
        
        // Replace password
        if (!string.IsNullOrEmpty(dto.Password))
        {
            existingUser.PasswordHash = _passwordHasher.HashPassword(existingUser, dto.Password);
            existingUser.UserType = UserType.Registered;
        }

        _repository.Update(existingUser);
    }
}