using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaApp.Model;

public class Cinema : EntityBase
{
    [Required, StringLength(100)]
    public string Name { get; set; } = null!;

    [Required, StringLength(200)]
    public string Address { get; set; } = null!;

    [StringLength(50)]
    public string? City { get; set; }

    public ICollection<Room> Rooms { get; } = new List<Room>();
}
