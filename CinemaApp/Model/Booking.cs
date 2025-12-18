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

    public decimal BasePrice { get; set; }
    public decimal DiscountedPrice { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public ICollection<AppliedOffer> AppliedOffers { get; set; } = new List<AppliedOffer>();
    
    // Payment tracking
    public string? PayUOrderId { get; set; }
    public string? PaymentTransactionId { get; set; }
    public decimal? PaymentAmount { get; set; }
    public DateTime? PaymentDate { get; set; }
}

public enum BookingStatus
{
    Confirmed,
    Pending,
    Cancelled
}