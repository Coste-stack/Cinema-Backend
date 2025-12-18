using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CinemaApp.DTO;

public class UserBookingDTO
{
    [Required]
    [JsonPropertyName("id")]
    public int BookingId { get; set; }

    [Required]
    [JsonPropertyName("screening")]
    public UserScreeningDTO Screening { get; set; } = null!;

    [Required]
    [JsonPropertyName("tickets")]
    public List<UserTicketDTO> Tickets { get; set; } = new();

    [Required]
    [JsonPropertyName("basePrice")]
    public decimal BasePrice { get; set; }

    [Required]
    [JsonPropertyName("discountedPrice")]
    public decimal DiscountedPrice { get; set; }
}
