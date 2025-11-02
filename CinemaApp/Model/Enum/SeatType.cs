using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class SeatType : LookupEntity
{
    [Required]
    [Range(0, double.MaxValue)]
    public decimal PriceAmountDiscount { get; set; }
}
