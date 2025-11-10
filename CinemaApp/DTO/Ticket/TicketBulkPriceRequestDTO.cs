using System.ComponentModel.DataAnnotations;

namespace CinemaApp.DTO.Ticket;

public class TicketBulkPriceRequestDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "ScreeningId must be a positive integer.")]
    public int ScreeningId { get; set; }
    
    [Required(ErrorMessage = "At least one ticket is required.")]
    [MinLength(1, ErrorMessage = "At least one ticket is required.")]
    public List<TicketPriceRequestDTO> Tickets { get; set; } = new();
}
