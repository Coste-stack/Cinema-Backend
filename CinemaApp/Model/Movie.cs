using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class Movie
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Duration must be positive")]
    public int Duration { get; set; }

    [StringLength(50)]
    public string? Genre { get; set; }

    [EnumDataType(typeof(MovieRating))]
    public MovieRating? Rating { get; set; }

    public DateTime? ReleaseDate { get; set; }
}

public enum MovieRating
{
    G,
    PG,
    PG13,
    R,
    NC17
}
