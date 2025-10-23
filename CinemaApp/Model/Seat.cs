using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class Seat
{
    [Key]
    public int Id { get; set; }

    // TODO: add row and number constraint unique
    [Required]
    public string? Row { get; set; }

    [Required]
    public int Number { get; set; }

    [Required]
    public SeatStatus Status { get; set; } = SeatStatus.Available;

    public int RoomId { get; set; }
    public Room? Room { get; set; }
}

// TODO: Convert to separate table
public enum SeatStatus
{
    Available,
    Reserved
}
