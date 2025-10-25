using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class Booking : EntityBase
{
    public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;

    [Required]
    public DateTime BookingTime { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int ScreeningId { get; set; }
    public Screening? Screening { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>(); 
}

public enum BookingStatus
{
    Confirmed,
    Pending,
    Cancelled
}