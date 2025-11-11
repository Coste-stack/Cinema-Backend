namespace CinemaApp.DTO;

public class SeatMapResponseDTO
{
    public int ScreeningId { get; set; }
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public List<SeatRowDTO> SeatMap { get; set; } = new();
}

public class SeatRowDTO
{
    public string Row { get; set; } = string.Empty;
    public List<SeatInfoDTO> Seats { get; set; } = new();
}

public class SeatInfoDTO
{
    public int Id { get; set; }
    public int Number { get; set; }
    public int SeatTypeId { get; set; }
    public string? SeatTypeName { get; set; }
    public bool IsAvailable { get; set; }
}
