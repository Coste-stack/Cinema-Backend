using System.ComponentModel.DataAnnotations;
using CinemaApp.Model;
using CinemaApp.DTO.Ticket;

namespace CinemaApp.DTO;

public class BookingCreateDto
{
    [Required]
    public DateTime BookingTime { get; set; }

    [Required]
    public int ScreeningId { get; set; }

    public UserType UserType { get; set; } = UserType.Guest;

    [Required]
    [MinLength(1)]
    public List<TicketCreateDto> Tickets { get; set; } = new List<TicketCreateDto>();
}