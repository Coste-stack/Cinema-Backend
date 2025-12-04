namespace CinemaApp.DTO.Ticket;

public class TicketPriceResponseDTO
{
    public int SeatId { get; set; }
    public required string PersonTypeName { get; set; }
    public decimal Price { get; set; }
}
