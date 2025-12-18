namespace CinemaApp.DTO.Ticket;

public class BookingPriceResponseDTO
{
    public int ScreeningId { get; set; }
    public List<TicketPriceResponseDTO> TicketPrices { get; set; } = new();
    public decimal BasePrice { get; set; }
    public decimal DiscountedPrice { get; set; }
    public List<AppliedOfferResponseDTO> AppliedOffers { get; set; } = new();
}