using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class Screening
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public ProjectionType ProjectionType { get; set; } = ProjectionType.TwoD;

    public string? Language { get; set; }

    [Required]
    [DataType(DataType.Currency)]
    public float Price { get; set; }

    public int RoomId { get; set; }
    public Room? Room { get; set; }

    public int MovieId { get; set; }
    public Movie? Movie { get; set; }
}

public enum ProjectionType
{
    [Description("3D")]
    ThreeD,

    [Description("2D")]
    TwoD
}
