using CinemaApp.Model;

namespace CinemaApp.DTO;

public class MovieDto {
  public int Id { get; set; }
  public string Title { get; set; } = "";
  public string? Description { get; set; }
  public int Duration { get; set; }
  public MovieRating? Rating { get; set; }
  public DateTime? ReleaseDate { get; set; }
  public List<string> Genres { get; set; } = new();
}

