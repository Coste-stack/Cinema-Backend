using System.ComponentModel.DataAnnotations;

namespace CinemaApp.DTO.Ticket;

public class TicketPriceRequestDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "SeatId must be a positive integer.")]
    public int SeatId { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "PersonTypeId must be a positive integer.")]
    public int PersonTypeId { get; set; }
}