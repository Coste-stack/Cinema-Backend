using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class Screening : EntityBase
{
    [Required]
    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? Language { get; set; }

    [Required]
    public int ProjectionTypeId { get; set; }
    public ProjectionType? ProjectionType { get; set; }

    public int RoomId { get; set; }
    public Room? Room { get; set; }

    public int MovieId { get; set; }
    public Movie? Movie { get; set; }
}
