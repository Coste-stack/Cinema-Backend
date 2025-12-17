using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Model;

public class Offer : EntityBase
{
    [Required]
    public string? Name { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public int Priority { get; set; }

    public bool IsStackable { get; set; } = true;
    public ICollection<OfferCondition> Conditions { get; set; } = new List<OfferCondition>();
    public ICollection<OfferEffect> Effects { get; set; } = new List<OfferEffect>();
    public ICollection<AppliedOffer> AppliedOffers { get; set; } = new List<AppliedOffer>();
}
