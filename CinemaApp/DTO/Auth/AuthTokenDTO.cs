namespace CinemaApp.Model;

public class AuthTokenDTO
{
    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }
}
