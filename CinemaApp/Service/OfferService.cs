using System;
using System.Collections.Generic;
using System.Linq;
using CinemaApp.DTO.Ticket;
using CinemaApp.Model;
using CinemaApp.DTO.Offer;
using CinemaApp.Repository;

namespace CinemaApp.Service;

public interface IOfferService
{
    List<OfferDTO> GetAllOffers(bool includeApplied = false);
    List<OfferDTO> GetActiveOffers(bool includeApplied = false);
    OfferDTO? GetById(int id, bool includeApplied = false);
    bool IsOfferValid(Offer offer, DateTime now);
    decimal CalculateDiscountAmount(Offer offer, decimal basePrice);
    decimal SumAppliedOffers(IEnumerable<AppliedOffer>? offers);
    List<AppliedOffer> GetAppliedOffers(int screeningId, List<TicketPriceRequestDTO> tickets, int bookingId = 0);
    void ApplyOffersToBooking(int bookingId, int screeningId, List<TicketPriceRequestDTO> tickets);
    decimal GetBookingDiscountAmount(int bookingId);
}

public class OfferService(IOfferRepository repo, IScreeningRepository screeningRepo, IMovieRepository movieRepo, IPriceCalculationService priceService) : IOfferService
{
    private readonly IOfferRepository _repo = repo;
    private readonly IScreeningRepository _screeningRepo = screeningRepo;
    private readonly IMovieRepository _movieRepo = movieRepo;
    private readonly IPriceCalculationService _priceService = priceService;

    public List<OfferDTO> GetAllOffers(bool includeApplied = false)
    {
        var offers = _repo.GetAllOffers(includeApplied);
        return offers.Select(o => MapToDto(o, includeApplied)).ToList();
    }

    public List<OfferDTO> GetActiveOffers(bool includeApplied = false)
    {
        var offers = _repo.GetActiveOffers(includeApplied);
        return offers.Select(o => MapToDto(o, includeApplied)).ToList();
    }

    public OfferDTO? GetById(int id, bool includeApplied = false)
    {
        var offer = _repo.Get(id, includeApplied);
        if (offer == null) return null;
        return MapToDto(offer, includeApplied);
    }
    private OfferDTO MapToDto(Offer o, bool includeApplied)
    {
        if (o == null) return null!;
        return new OfferDTO
        {
            Id = o.Id,
            Name = o.Name,
            Description = o.Description,
            IsActive = o.IsActive,
            ValidFrom = o.ValidFrom,
            ValidTo = o.ValidTo,
            Priority = o.Priority,
            IsStackable = o.IsStackable,
            Conditions = o.Conditions?.Select(c => new OfferConditionDTO
            {
                ConditionType = c.ConditionType ?? string.Empty,
                ConditionValue = c.ConditionValue ?? string.Empty
            }).ToList() ?? new List<OfferConditionDTO>(),
            Effects = o.Effects?.Select(e => new OfferEffectDTO
            {
                EffectType = e.EffectType ?? string.Empty,
                EffectValue = e.EffectValue
            }).ToList() ?? new List<OfferEffectDTO>(),
            AppliedOffers = includeApplied
                ? (o.AppliedOffers?.Select(a => new AppliedOfferDTO
                {
                    BookingId = a.BookingId,
                    DiscountAmount = a.DiscountAmount
                }).ToList() ?? new List<AppliedOfferDTO>())
                : null
        };
    }

    public bool IsOfferValid(Offer offer, DateTime now)
    {
        if (offer == null) throw new ArgumentNullException(nameof(offer));
        if (!offer.IsActive) return false;
        if (offer.ValidFrom.HasValue && offer.ValidFrom.Value > now) return false;
        if (offer.ValidTo.HasValue && offer.ValidTo.Value < now) return false;
        return true;
    }

    public decimal CalculateDiscountAmount(Offer offer, decimal basePrice)
    {
        if (offer == null) throw new ArgumentNullException(nameof(offer));
        if (offer.Effects == null || offer.Effects.Count == 0) return 0m;

        decimal amountTotal = 0m;
        decimal percentTotal = 0m;

        foreach (var e in offer.Effects)
        {
            var t = (e.EffectType ?? string.Empty).ToLowerInvariant();
            if (t.Contains("fixed"))
            {
                amountTotal += e.EffectValue;
            }
            else if (t.Contains("percent") || t.Contains("percentage"))
            {
                percentTotal += e.EffectValue;
            }
        }

        decimal percentAmount = basePrice * (percentTotal / 100m);
        var total = amountTotal + percentAmount;
        if (total < 0) total = 0m;
        if (total > basePrice) total = basePrice;
        return total;
    }

    public decimal SumAppliedOffers(IEnumerable<AppliedOffer>? offers)
    {
        return offers?.Sum(a => a.DiscountAmount) ?? 0m;
    }

    public List<AppliedOffer> GetAppliedOffers(int screeningId, List<TicketPriceRequestDTO> tickets, int bookingId = 0)
    {
        var offers = _repo.GetActiveOffers();
        var now = DateTime.UtcNow;

        var appliedOffers = new List<AppliedOffer>();
        var orderedOffers = offers.OrderByDescending(o => o.Priority).ToList();

        var screening = _screeningRepo.GetById(screeningId);
        var referenceDay = screening?.StartTime.DayOfWeek ?? DateTime.UtcNow.DayOfWeek;

        bool appliedNonStackable = false;

        foreach(var offer in orderedOffers) 
        {
            if (!IsOfferValid(offer, now)) continue;
            if (appliedNonStackable) break;
            
            bool allConditionMatch = true;
            bool dayOfWeekMatched = false;
            bool hasDayOfWeekCondition = false;

            // Check if all conditions apply
            foreach(var condition in offer.Conditions) {
                if (condition.ConditionType == "DayOfWeek") 
                {
                    if (Enum.TryParse<DayOfWeek>(condition.ConditionValue, true, out var conditionDay))
                    {
                        hasDayOfWeekCondition = true;
                        if (referenceDay == conditionDay) {
                            dayOfWeekMatched = true;
                        }
                    }
                    continue;
                }
                else if (condition.ConditionType == "MinimumTicketCount") 
                {
                    if (!int.TryParse(condition.ConditionValue, out var val) || tickets.Count < val) {
                        allConditionMatch = false;
                        break;
                    }
                } 
                else 
                {
                    allConditionMatch = false;
                    break;
                }
            }

            // Evaluate DayOfWeek conditions by OR operation
            if (hasDayOfWeekCondition && !dayOfWeekMatched) {
                allConditionMatch = false;
            }

            if (!allConditionMatch) {
                continue;
            }

            var bookingRequest = new BookingPriceRequestDTO
            {
                ScreeningId = screeningId,
                BookingId = bookingId,
                BookingTime = DateTime.UtcNow,
                Tickets = tickets
            };

            var bookingPriceResponse = _priceService.CalculateBookingPrice(bookingRequest);
            var bookingPrice = bookingPriceResponse.TotalPrice;
            var discount = CalculateDiscountAmount(offer, bookingPrice);
            if (discount <= 0m) continue;

            // Apply offer to booking
            appliedOffers.Add(new AppliedOffer
            {
                OfferId = offer.Id,
                BookingId = bookingId > 0 ? bookingId : null,
                DiscountAmount = Math.Min(discount, bookingPrice)
            });

            if (!offer.IsStackable) {
                appliedNonStackable = true;
                break;
            }
        }

        return appliedOffers;
    }

    public void ApplyOffersToBooking(int bookingId, int screeningId, List<TicketPriceRequestDTO> tickets)
    {
        var appliedOffers = GetAppliedOffers(screeningId, tickets, bookingId);
        if (appliedOffers.Count > 0) {
            foreach (var ao in appliedOffers)
            {
                if (ao.BookingId == null) ao.BookingId = bookingId > 0 ? bookingId : null;
            }
            _repo.AddAppliedOffers(appliedOffers);
        }
    }

    public decimal GetBookingDiscountAmount(int bookingId) {
        decimal totalDiscount = 0;
        var appliedOffers = _repo.GetAppliedOffers(bookingId);
        foreach(var offer in appliedOffers)
        {
            totalDiscount += offer.DiscountAmount;
        }
        return totalDiscount;
    }
}
