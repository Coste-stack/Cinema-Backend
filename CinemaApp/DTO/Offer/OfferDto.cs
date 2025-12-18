namespace CinemaApp.DTO.Offer;

public class OfferDTO
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public int Priority { get; set; }

    public bool IsStackable { get; set; } = true;

    public ICollection<OfferConditionDTO> Conditions { get; set; } = new List<OfferConditionDTO>();
    public ICollection<OfferEffectDTO> Effects { get; set; } = new List<OfferEffectDTO>();
    public ICollection<AppliedOfferDTO>? AppliedOffers { get; set; } = new List<AppliedOfferDTO>();
}
