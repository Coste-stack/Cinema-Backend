using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class Room
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public int Capacity => Seats.Count;

    public int CinemaId { get; set; }
    public Cinema? Cinema { get; set; }

    public ICollection<Seat> Seats { get; } = new List<Seat>();

    public ICollection<Screening> Screenings { get; } = new List<Screening>();
}
