using CinemaApp.Model;

namespace CinemaApp.DTO;

public class LatestMovieDTO {
  public MovieDto Movie { get; set; } = new MovieDto();
  public DateTime? ReleaseDate { get; set; }
}