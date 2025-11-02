using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class ProjectionType : LookupEntity
{
    [Required]
    [Range(0, double.MaxValue)]
    public decimal PriceAmountDiscount { get; set; }
}
