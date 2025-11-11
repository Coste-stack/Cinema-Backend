using System.ComponentModel.DataAnnotations;
using CinemaApp.DTO.Ticket;

namespace CinemaApp.DTO;

public class BookingRequestDTO
{
    [Required]
    public DateTime BookingTime { get; set; }

    [Required]
    public int ScreeningId { get; set; }

    // Optional user id for authenticated users
    public int? UserId { get; set; }

    [Required]
    [MinLength(1)]
    public List<TicketCreateDto> Tickets { get; set; } = new();
}
