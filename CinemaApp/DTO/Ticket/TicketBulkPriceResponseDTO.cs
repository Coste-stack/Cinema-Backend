namespace CinemaApp.DTO.Ticket;

public class TicketBulkPriceResponseDTO
{
    public int ScreeningId { get; set; }
    public List<TicketPriceResponseDTO> TicketPrices { get; set; } = new();
    public decimal TotalPrice { get; set; }
}