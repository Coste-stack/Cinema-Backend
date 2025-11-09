using System.ComponentModel.DataAnnotations;
using CinemaApp.Model.Attributes;

namespace CinemaApp.Model;

public class Movie : EntityBase
{
    [Required]
    [StringLength(100)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Duration must be positive")]
    public int Duration { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Base Price must be positive")]
    public decimal BasePrice { get; set; }

    [MinimumCount(1, ErrorMessage = "At least one genre must be specified.")]
    public ICollection<Genre> Genres { get; set; } = new List<Genre>();

    [EnumDataType(typeof(MovieRating))]
    public MovieRating? Rating { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public ICollection<Screening> Screenings { get; } = new List<Screening>();
}

public enum MovieRating
{
    G,
    PG,
    PG13,
    R,
    NC17
}
