using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class Room
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [Range(1, 500)]
    public int Capacity { get; set; }

    public int CinemaId { get; set; }
    public Cinema? Cinema { get; set; }

    public ICollection<Seat> Seats { get; } = new List<Seat>();

    public ICollection<Screening> Screenings { get; } = new List<Screening>();
}
