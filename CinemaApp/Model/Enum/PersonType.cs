using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class PersonType : LookupEntity
{
    [Required]
    [Range(0, 100)]
    public decimal PricePercentDiscount { get; set; }
}
