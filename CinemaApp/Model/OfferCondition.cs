using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class OfferCondition : EntityBase
{
    [Required]
    public int OfferId { get; set; }
    public Offer Offer { get; set; } = null!;

    [Required]
    public string? ConditionType { get; set; }

    [Required]
    public string? ConditionValue { get; set; }
}
