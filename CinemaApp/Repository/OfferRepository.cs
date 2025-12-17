using CinemaApp.Data;
using CinemaApp.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace CinemaApp.Repository;

public interface IOfferRepository
{
    List<Offer> GetActiveOffers();
    void AddAppliedOffers(IEnumerable<AppliedOffer> offers);
    List<AppliedOffer> GetAppliedOffers(int bookingId);
}

public class OfferRepository : IOfferRepository
{
    private readonly AppDbContext _context;

    public OfferRepository(AppDbContext context) => _context = context;

    public List<Offer> GetActiveOffers()
    {
        return _context.Offers
            .Include(o => o.Conditions)
            .Include(o => o.Effects)
            .AsNoTracking()
            .Where(o => o.IsActive)
            .ToList();
    }

    public void AddAppliedOffers(IEnumerable<AppliedOffer> offers)
    {
        if (offers == null) return;
        _context.AppliedOffers.AddRange(offers);
        try
        {
            var affected = _context.SaveChanges();
            if (affected == 0)
                throw new ConflictException("No rows affected when adding applied offers.");
            return;
        }
        catch (DbUpdateException ex)
        {
            throw new ConflictException("Database update failed when adding applied offers.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Unexpected error when adding applied offers.", ex);
        }
    }

    public List<AppliedOffer> GetAppliedOffers(int bookingId)
    {
        return _context.AppliedOffers
            .Include(a => a.Offer)
                .ThenInclude(o => o.Conditions)
            .Include(a => a.Offer)
                .ThenInclude(o => o.Effects)
            .Include(a => a.Booking)
            .AsNoTracking()
            .Where(a => a.BookingId == bookingId)
            .ToList();
    }
}
