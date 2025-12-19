namespace CinemaApp.DTO.Turnstile;

public class TurnstileResponseDTO
{
    public bool Success { get; set; }
    public string[]? ErrorCodes { get; set; }
}