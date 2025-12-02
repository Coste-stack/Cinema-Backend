using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class User : EntityBase
{
    public UserType UserType { get; set; }

    public DateTime CreatedAt { get; set; }

    [Required]
    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; } = new List<RefreshToken>();

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

public class RefreshToken
{
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool Invalidated { get; set; } = false;
}
