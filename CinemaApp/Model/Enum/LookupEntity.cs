using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public abstract class LookupEntity : EntityBase
{
    [Required]
    public string Name { get; set; } = null!;
}
