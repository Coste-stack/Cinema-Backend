using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class User
{
    [Key]
    public int Id { get; set; }

    public UserType UserType { get; set; }

    public DateTime CreatedAt { get; set; }

    [Required]
    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public ICollection<Booking> Bookings { get; } = new List<Booking>(); 

    public User()
    {
        CreatedAt = DateTime.UtcNow;
        UserType = UserType.Guest;
    }
}

public enum UserType
{
    Registered,
    Guest
}
