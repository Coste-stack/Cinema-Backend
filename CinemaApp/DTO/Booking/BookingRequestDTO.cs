using System.ComponentModel.DataAnnotations;
using CinemaApp.DTO.Ticket;

namespace CinemaApp.DTO;

public class BookingRequestDTO
{
    [Required]
    public int ScreeningId { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    [MinLength(1)]
    public List<TicketCreateDto> Tickets { get; set; } = new();
}
