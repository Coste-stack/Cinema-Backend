namespace CinemaApp.DTO.Ticket;

public class BookingPriceResponseDTO
{
    public int ScreeningId { get; set; }
    public List<TicketPriceResponseDTO> TicketPrices { get; set; } = new();
    public decimal TotalPrice { get; set; }
    public List<AppliedOfferResponseDTO> AppliedOffers { get; set; } = new();
}