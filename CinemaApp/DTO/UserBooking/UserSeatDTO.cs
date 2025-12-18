using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CinemaApp.DTO;

public class UserSeatDTO
{
    [Required]
    [JsonPropertyName("id")]
    public int SeatId { get; set; }
    
    [Required]
    [JsonPropertyName("row")]
    public string Row { get; set; } = null!;

    [Required]
    [JsonPropertyName("number")]
    public int Number { get; set; }
    
    [Required]
    [JsonPropertyName("seatType")]
    public string SeatTypeName { get; set; } = null!;
}
