using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CinemaApp.Model;

namespace CinemaApp.DTO;

public class UserMovieDTO
{
    [Required]
    [JsonPropertyName("id")]
    public int MovieId { get; set; }

    [Required]
    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;
    
    [JsonPropertyName("genres")]
    public ICollection<Genre> Genres { get; set; } = new List<Genre>();

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("releaseDate")]
    public string? ReleaseDate { get; set; }

    [EnumDataType(typeof(MovieRating))]
    [JsonPropertyName("rating")]
    public MovieRating? Rating { get; set; }
}
