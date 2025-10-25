namespace CinemaApp.Model;

public class UserResponseDTO
{
    public int Id { get; set; }

    public UserType UserType { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Email { get; set; } = null!;
}
