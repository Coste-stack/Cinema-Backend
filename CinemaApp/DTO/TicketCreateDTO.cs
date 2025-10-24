using System.ComponentModel.DataAnnotations;

namespace CinemaApp.DTO;

public class TicketCreateDto
{
    [Required]
    public int SeatId { get; set; }
    
    [Required]
    public int PersonTypeId { get; set; }
    
    // TODO: add ticketPriceId
    // [Required] 
    // public int TicketPriceId { get; set; }
}