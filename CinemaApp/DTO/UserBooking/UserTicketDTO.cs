using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CinemaApp.DTO.Ticket;

namespace CinemaApp.DTO;

public class UserTicketDTO
{
    [Required]
    [JsonPropertyName("id")]
    public int TicketId { get; set; }

    [Required]
    [JsonPropertyName("totalPrice")]
    public decimal TotalPrice { get; set; }
    
    [Required]
    [JsonPropertyName("personType")]
    public string PersonTypeName { get; set; } = null!;

    [Required]
    [JsonPropertyName("seat")]
    public UserSeatDTO Seat { get; set; } = null!;
}
