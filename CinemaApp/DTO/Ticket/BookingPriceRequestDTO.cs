using System.ComponentModel.DataAnnotations;

namespace CinemaApp.DTO.Ticket;

public class BookingPriceRequestDTO
{
    public int BookingId { get; set; } = 0;
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "ScreeningId must be a positive integer.")]
    public int ScreeningId { get; set; }

    public DateTime BookingTime { get; set; }
    
    [Required(ErrorMessage = "At least one ticket is required.")]
    [MinLength(1, ErrorMessage = "At least one ticket is required.")]
    public List<TicketPriceRequestDTO> Tickets { get; set; } = new();
}
