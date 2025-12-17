using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class OfferEffect : EntityBase
{
    [Required]
    public int OfferId { get; set; }
    public Offer Offer { get; set; } = null!;

    [Required]
    public string? EffectType { get; set; }

    [Required]
    public decimal EffectValue { get; set; }
}
