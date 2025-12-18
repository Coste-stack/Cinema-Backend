using CinemaApp.Data;
using CinemaApp.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace CinemaApp.Repository;

public interface IOfferRepository
{
    List<Offer> GetAllOffers(bool includeApplied = false);
    List<Offer> GetActiveOffers(bool includeApplied = false);
    Offer? Get(int id, bool includeApplied = false);
    void AddAppliedOffers(IEnumerable<AppliedOffer> offers);
    List<AppliedOffer> GetAppliedOffers(int bookingId);
}

public class OfferRepository : IOfferRepository
{
    private readonly AppDbContext _context;

    public OfferRepository(AppDbContext context) => _context = context;

    public List<Offer> GetAllOffers(bool includeApplied = false)
    {
        IQueryable<Offer> q = _context.Offers
            .Include(o => o.Conditions)
            .Include(o => o.Effects);

        if (includeApplied)
            q = q.Include(o => o.AppliedOffers);

        return q.AsNoTracking().ToList();
    }

    public List<Offer> GetActiveOffers(bool includeApplied = false)
    {
        IQueryable<Offer> q = _context.Offers
            .Include(o => o.Conditions)
            .Include(o => o.Effects)
            .Where(o => o.IsActive);

        if (includeApplied)
            q = q.Include(o => o.AppliedOffers);

        return q.AsNoTracking().ToList();
    }

    public Offer? Get(int id, bool includeApplied = false)
    {
        IQueryable<Offer> q = _context.Offers
            .Include(x => x.Conditions)
            .Include(x => x.Effects);

        if (includeApplied)
            q = q.Include(x => x.AppliedOffers);

        return q.AsNoTracking().FirstOrDefault(x => x.Id == id);
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
