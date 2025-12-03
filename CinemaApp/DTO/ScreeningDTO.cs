using CinemaApp.Model;

namespace CinemaApp.DTO;

public class ScreeningDto {
    public DateTime StartTime { get; set; }
    public string? Language { get; set; }
    public string ProjectionType { get; set; } = null!;
}