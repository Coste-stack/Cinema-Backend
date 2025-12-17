using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class AppliedOffer : EntityBase
{
    [Required]
    public int OfferId { get; set; }
    public Offer Offer { get; set; } = null!;

    public int? BookingId { get; set; }
    public Booking? Booking { get; set; }

    [Required]
    public decimal DiscountAmount { get; set; }
}
