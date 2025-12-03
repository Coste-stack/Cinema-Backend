using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class UserCreateDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    public string? Password { get; set; }
}
