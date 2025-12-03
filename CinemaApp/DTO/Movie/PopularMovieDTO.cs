using CinemaApp.Model;

namespace CinemaApp.DTO;

public class PopularMovieDTO {
  public MovieDto Movie { get; set; } = new MovieDto();
  public int TicketsSold { get; set; }
}