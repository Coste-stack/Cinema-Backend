using System.ComponentModel.DataAnnotations;

namespace CinemaApp.DTO.Ticket;

public class TicketCreateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "SeatId must be a positive integer.")]
    public int SeatId { get; set; }
    
    [Required(ErrorMessage = "PersonTypeName is required.")]
    public string PersonTypeName { get; set; } = null!;
}