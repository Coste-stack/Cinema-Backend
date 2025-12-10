using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CinemaApp.DTO;

public class UserScreeningDTO
{
    [Required]
    [JsonPropertyName("id")]
    public int ScreeningId { get; set; }

    [Required]
    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

    [Required]
    [JsonPropertyName("endTime")]
    public DateTime EndTime { get; set; }

    [Required]
    [JsonPropertyName("movie")]
    public UserMovieDTO Movie { get; set; } = null!;

    [Required]
    [JsonPropertyName("projectionType")]
    public string ProjectionType { get; set; } = null!;

    [JsonPropertyName("language")]
    public string? Language { get; set; }
}
