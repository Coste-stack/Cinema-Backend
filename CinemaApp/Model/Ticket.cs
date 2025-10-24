using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class Ticket
{
    [Key]
    public int Id { get; set; }

    public int PersonTypeId { get; set; }
    public PersonType? PersonType { get; set; }

    // TODO: add ticketPriceId

    public int BookingId { get; set; }
    public Booking? Booking { get; set; }

    public int SeatId { get; set; }
    public Seat? Seat { get; set; }

    // for enforcing uniqueness of seat per screening at repository level
    public int ScreeningId { get; set; }
    public Screening? Screening { get; set; }
}
