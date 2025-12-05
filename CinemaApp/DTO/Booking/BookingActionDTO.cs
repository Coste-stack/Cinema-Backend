using System.ComponentModel.DataAnnotations;

namespace CinemaApp.DTO;

public class BookingActionDTO
{
    [EmailAddress]
    public string? Email { get; set; }
}
