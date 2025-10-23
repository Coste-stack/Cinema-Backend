using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class ProjectionType
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;
}
