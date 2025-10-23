using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class Ticket
{
    [Key]
    public int Id { get; set; }

    // TODO: remove bookingTime - move it to booking (parent) table
    // [Required]
    // public DateTime BookingTime { get; set; }

    // TODO: remove screening - move it to booking (parent) table
    // public int ScreeningId { get; set; }
    // public Screening? Screening { get; set; }

    public int PersonTypeId { get; set; }
    public PersonType? PersonType { get; set; }

    // TODO: add ticketPriceId

    public int SeatId { get; set; }
    public Seat? Seat { get; set; }
}
