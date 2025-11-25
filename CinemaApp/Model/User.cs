using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class User : EntityBase
{
    public UserType UserType { get; set; }

    public DateTime CreatedAt { get; set; }

    [Required]
    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public ICollection<Booking> Bookings { get; } = new List<Booking>(); 

    public User()
    {
        UserType = UserType.Guest;
    }
}

public enum UserType
{
    Registered,
    Guest
}
