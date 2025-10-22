using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class Seat
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string? Row { get; set; }

    [Required]
    public int Number { get; set; }

    public int RoomId { get; set; }
    public Room? Room { get; set; }
}
