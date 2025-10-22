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

    [Required]
    public SeatStatus Status { get; set; } = SeatStatus.Available;

    public int RoomId { get; set; }
    public Room? Room { get; set; }
}

public enum SeatStatus
{
    Available,
    Reserved
}
