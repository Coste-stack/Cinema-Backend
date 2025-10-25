using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class Room : EntityBase
{
    [Required]
    public string Name { get; set; } = null!;

    // TODO: update the functionality (because it can change after constructing)
    [Range(0, int.MaxValue)]
    public int Capacity => Seats.Count;

    public int CinemaId { get; set; }
    public Cinema? Cinema { get; set; }

    public ICollection<Seat> Seats { get; } = new List<Seat>();

    public ICollection<Screening> Screenings { get; } = new List<Screening>();

    //public void RecalculateCapacity() => Capacity = Seats?.Count ?? 0;
}
