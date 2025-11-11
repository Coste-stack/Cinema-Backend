using CinemaApp.Configuration;
using CinemaApp.Data;
using CinemaApp.Model;
using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Service;

public class BookingExpiryBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingExpiryBackgroundService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

    public BookingExpiryBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<BookingExpiryBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Booking Expiry Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CancelExpiredBookingsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cancelling expired bookings.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }

        _logger.LogInformation("Booking Expiry Background Service stopped.");
    }

    private async Task CancelExpiredBookingsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var expiryThreshold = DateTime.UtcNow.Subtract(BookingConfiguration.PendingBookingHoldDuration);

        var expiredBookings = await context.Bookings
            .Where(b => b.BookingStatus == BookingStatus.Pending &&
                        b.BookingTime < expiryThreshold)
            .ToListAsync();

        if (expiredBookings.Any())
        {
            _logger.LogInformation($"Cancelling {expiredBookings.Count} expired pending booking(s).");

            foreach (var booking in expiredBookings)
            {
                booking.BookingStatus = BookingStatus.Cancelled;
            }

            await context.SaveChangesAsync();
        }
    }
}
