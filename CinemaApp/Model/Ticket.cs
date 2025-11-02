using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class Ticket : EntityBase
{
    public int PersonTypeId { get; set; }
    public PersonType? PersonType { get; set; }

    public decimal TotalPrice { get; set; }

    [Required]
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    [Required]
    public int SeatId { get; set; }
    public Seat Seat { get; set; } = null!;

    public int ScreeningId { get; set; }
    public Screening? Screening { get; set; }

}
