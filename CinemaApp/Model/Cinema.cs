using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class Cinema
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = null!;

    [Required, StringLength(200)]
    public string Address { get; set; } = null!;

    [StringLength(50)]
    public string? City { get; set; }

    public ICollection<Room> Rooms { get; } = new List<Room>();
}
