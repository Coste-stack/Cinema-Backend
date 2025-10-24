using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class Booking
{
    [Key]
    public int Id { get; set; }

    public UserType UserType { get; set; } = UserType.Guest;

    public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;

    // TODO: Add user table
    // public int UserId { get; set; }
    // public User User { get; set; }

    [Required]
    public DateTime BookingTime { get; set; }

    public int ScreeningId { get; set; }
    public Screening? Screening { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>(); 
}

public enum UserType
{
    Registered,
    Guest
}

public enum BookingStatus
{
    Confirmed,
    Pending,
    Cancelled
}