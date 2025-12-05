using System.Security.Claims;
using CinemaApp.Model;
using CinemaApp.Service;

namespace CinemaApp.Helpers;

public static class BookingAuthorization
{
    /// <summary>
    /// Returns true if caller (identified by <paramref name="user"/> or by <paramref name="guestEmail"/>)
    /// is authorized to act on the provided booking.
    /// </summary>
    public static bool IsAuthorized(ClaimsPrincipal? user, IUserService userService, Booking booking, string? guestEmail = null)
    {
        if (booking == null) return false;

        // If authenticated, check JWT subject/NameIdentifier
        if (user?.Identity != null && user.Identity.IsAuthenticated)
        {
            var sub = user.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                      ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(sub, out var userId))
            {
                return booking.UserId == userId;
            }
        }

        // Otherwise, allow if guestEmail matches the booking owner's email
        if (!string.IsNullOrWhiteSpace(guestEmail) && booking.UserId > 0)
        {
            var owner = userService.Get(booking.UserId);
            if (owner != null && string.Equals(owner.Email, guestEmail, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
