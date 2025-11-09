namespace CinemaApp.Model;

public class Genre : LookupEntity
{
    public ICollection<Movie> Movies { get; set; } = new List<Movie>();
}
