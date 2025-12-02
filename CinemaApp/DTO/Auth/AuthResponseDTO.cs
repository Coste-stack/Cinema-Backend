namespace CinemaApp.Model;

public class AuthResponseDTO
{
    public required AuthTokenDTO Token { get; set; }
    public required RefreshToken RefreshToken { get; set; }
}
