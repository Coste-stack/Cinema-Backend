using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class Seat : EntityBase
{
    // TODO: add row and number constraint unique
    [Required]
    public string? Row { get; set; }

    [Required]
    public int Number { get; set; }

    [Required]
    public int SeatTypeId { get; set; }
    public SeatType? SeatType { get; set; }

    public int RoomId { get; set; }
    public Room? Room { get; set; }
}

public enum SeatStatus
{
    Available,
    Reserved
}
