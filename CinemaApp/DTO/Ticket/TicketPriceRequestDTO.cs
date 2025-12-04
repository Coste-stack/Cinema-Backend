using System.ComponentModel.DataAnnotations;

namespace CinemaApp.DTO.Ticket;

public class TicketPriceRequestDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "SeatId must be a positive integer.")]
    public int SeatId { get; set; }
    
    [Required(ErrorMessage = "PersonTypeName is required.")]
    [MinLength(1, ErrorMessage = "PersonTypeName cannot be empty.")]
    public required string PersonTypeName { get; set; }
}