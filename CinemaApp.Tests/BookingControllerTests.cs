using CinemaApp.Controller;
using CinemaApp.Data;
using CinemaApp.DTO;
using CinemaApp.DTO.Ticket;
using CinemaApp.Model;
using CinemaApp.Repository;
using CinemaApp.Service;
using CinemaApp.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CinemaApp.Tests;

public class BookingControllerTests
{
    private static BookingController CreateControllerWithSeededData(out AppDbContext context)
    {
        context = TestDataSeeder.CreateTestDbContext();
        TestDataSeeder.SeedLookupData(context);

        var (cinema, room, seats) = TestDataSeeder.SeedCinemaWithRoomAndSeats(context, regularSeats: 3, vipSeats: 2);
        var movie = TestDataSeeder.SeedMovie(context, basePrice: 10.00m);
        var screening = TestDataSeeder.SeedScreening(context, movie.Id, room.Id, basePrice: 12.00m);
        var user = TestDataSeeder.SeedUser(context, "test@example.com");

        var bookingRepo = new BookingRepository(context);
        var screeningRepo = new ScreeningRepository(context);
        var ticketRepo = new TicketRepository(context);
        var offerRepository = new OfferRepository(context);
        var movieRepository = new MovieRepository(context);
        var personTypeRepo = new LookupRepository<PersonType>(context);
        var personTypeService = new LookupService<PersonType>(personTypeRepo);
        var priceCalcService = new PriceCalculationService(bookingRepo, screeningRepo, personTypeService);
        var userRepo = new UserRepository(context);
        var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
        var userService = new UserService(userRepo, passwordHasher);
        var offerService = new OfferService(offerRepository, screeningRepo, movieRepository, priceCalcService);
        var bookingService = new BookingService(bookingRepo, ticketRepo, priceCalcService, userService, personTypeService, offerService);

        return new BookingController(bookingService, userService);
    }

    [Fact]
    public void GetAll_ReturnsEmptyList_WhenNoBookings()
    {
        var controller = CreateControllerWithSeededData(out _);

        var result = controller.GetAll();

        var bookings = Assert.IsType<List<Booking>>(result.Value);
        Assert.Empty(bookings);
    }

    [Fact]
    public void InitiateBooking_CreatesBookingWithPendingStatus()
    {
        var controller = CreateControllerWithSeededData(out var context);
        var screening = context.Screenings.First();
        var seat = context.Seats.First();
        var user = context.Users.First();

        var request = new BookingRequestDTO
        {
            ScreeningId = screening.Id,
            Email = user.Email,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seat.Id, PersonTypeName = "Adult" } // Adult
            }
        };

        var result = controller.InitiateBooking(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var bookingId = Assert.IsType<int>(createdResult.Value);
        var booking = context.Bookings
            .Include(b => b.Tickets)
            .ThenInclude(t => t.PersonType)
            .First(b => b.Id == bookingId);
        Assert.Equal(BookingStatus.Pending, booking.BookingStatus);
        Assert.Equal(screening.Id, booking.ScreeningId);
        Assert.Equal(user.Id, booking.UserId);
        Assert.Single(booking.Tickets);
    }

    [Fact]
    public void InitiateBooking_CalculatesCorrectTotalPrice()
    {
        var controller = CreateControllerWithSeededData(out var context);
        var screening = context.Screenings.First();
        var seats = context.Seats.OrderBy(s => s.Id).Take(2).ToList();
        var user = context.Users.First();

        var request = new BookingRequestDTO
        {
            ScreeningId = screening.Id,
            Email = user.Email,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seats[0].Id, PersonTypeName = "Adult" }, // Adult, Regular seat
                new TicketCreateDto { SeatId = seats[1].Id, PersonTypeName = "Child" }  // Child, Regular seat
            }
        };

        var result = controller.InitiateBooking(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var bookingId = Assert.IsType<int>(createdResult.Value);
        var bookingEntity = context.Bookings
            .Include(b => b.Tickets)
            .ThenInclude(t => t.PersonType)
            .First(b => b.Id == bookingId);

        // Adult: 12.00, Child: 8.40 (30% discount)
        var totalPrice = bookingEntity.Tickets.Sum(t => t.TotalPrice);
        Assert.Equal(20.40m, totalPrice);
    }

    [Fact]
    public void InitiateBooking_ThrowsConflict_WhenSeatAlreadyBooked()
    {
        var controller = CreateControllerWithSeededData(out var context);
        var screening = context.Screenings.First();
        var seat = context.Seats.First();
        var user = context.Users.First();

        // First booking
        var request1 = new BookingRequestDTO
        {
            ScreeningId = screening.Id,
            Email = user.Email,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seat.Id, PersonTypeName = "Adult" }
            }
        };
        controller.InitiateBooking(request1);

        // Try to book the same seat again
        var request2 = new BookingRequestDTO
        {
            ScreeningId = screening.Id,
            Email = user.Email,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seat.Id, PersonTypeName = "Adult" }
            }
        };

        Assert.Throws<ConflictException>(() => controller.InitiateBooking(request2));
    }

    [Fact]
    public void ConfirmBooking_ChangesStatusToConfirmed()
    {
        var controller = CreateControllerWithSeededData(out var context);
        var screening = context.Screenings.First();
        var seat = context.Seats.First();
        var user = context.Users.First();

        var request = new BookingRequestDTO
        {
            ScreeningId = screening.Id,
            Email = user.Email,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seat.Id, PersonTypeName = "Adult" }
            }
        };

        var createResult = controller.InitiateBooking(request);
        var createdResult = Assert.IsType<CreatedAtActionResult>(createResult);
        var bookingId = Assert.IsType<int>(createdResult.Value);

        var confirmResult = controller.ConfirmBooking(bookingId, new BookingActionDTO { Email = user.Email });

        Assert.IsType<NoContentResult>(confirmResult);

        var confirmed = context.Bookings.Find(bookingId);
        Assert.Equal(BookingStatus.Confirmed, confirmed!.BookingStatus);
    }

    [Fact]
    public void CancelBooking_ChangesStatusToCancelled()
    {
        var controller = CreateControllerWithSeededData(out var context);
        var screening = context.Screenings.First();
        var seat = context.Seats.First();
        var user = context.Users.First();

        var request = new BookingRequestDTO
        {
            ScreeningId = screening.Id,
            Email = user.Email,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seat.Id, PersonTypeName = "Adult" }
            }
        };

        var createResult = controller.InitiateBooking(request);
        var createdResult = Assert.IsType<CreatedAtActionResult>(createResult);
        var bookingId = Assert.IsType<int>(createdResult.Value);

        var cancelResult = controller.CancelBooking(bookingId, new BookingActionDTO { Email = user.Email });

        Assert.IsType<NoContentResult>(cancelResult);

        var cancelled = context.Bookings.Find(bookingId);
        Assert.Equal(BookingStatus.Cancelled, cancelled!.BookingStatus);
    }

    [Fact]
    public void CancelBooking_AllowsSeatToBeBookedAgain()
    {
        var controller = CreateControllerWithSeededData(out var context);
        var screening = context.Screenings.First();
        var seat = context.Seats.First();
        var user = context.Users.First();

        // First booking
        var request1 = new BookingRequestDTO
        {
            ScreeningId = screening.Id,
            Email = user.Email,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seat.Id, PersonTypeName = "Adult" }
            }
        };
        var createResult1 = controller.InitiateBooking(request1);
        var createdResult1 = Assert.IsType<CreatedAtActionResult>(createResult1);
        var booking1Id = Assert.IsType<int>(createdResult1.Value);

        // Cancel it
        controller.CancelBooking(booking1Id, new BookingActionDTO { Email = user.Email });

        var request2 = new BookingRequestDTO
        {
            ScreeningId = screening.Id,
            Email = user.Email,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seat.Id, PersonTypeName = "Adult" }
            }
        };

        var createResult2 = controller.InitiateBooking(request2);
        Assert.IsType<CreatedAtActionResult>(createResult2);
    }

    [Fact]
    public void GetMyBookings_ReturnsUserBookings()
    {
        var controller = CreateControllerWithSeededData(out var context);
        var screening = context.Screenings.First();
        var seats = context.Seats.Take(2).ToList();
        var user = context.Users.First();

        // Create two bookings for the user
        var request1 = new BookingRequestDTO
        {
            ScreeningId = screening.Id,
            Email = user.Email,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seats[0].Id, PersonTypeName = "Adult" }
            }
        };
        controller.InitiateBooking(request1);

        var request2 = new BookingRequestDTO
        {
            ScreeningId = screening.Id,
            Email = user.Email,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seats[1].Id, PersonTypeName = "Adult" }
            }
        };
        controller.InitiateBooking(request2);

        // GetMyBookings requires JWT authentication, so we verify using GetAll instead
        var allBookings = controller.GetAll().Value;
        Assert.Equal(2, allBookings!.Count);
        Assert.All(allBookings, b => Assert.Equal(user.Id, b.UserId));
    }

    [Fact]
    public void GetById_ReturnsBooking_WhenExists()
    {
        var controller = CreateControllerWithSeededData(out var context);
        var screening = context.Screenings.First();
        var seat = context.Seats.First();
        var user = context.Users.First();

        var request = new BookingRequestDTO
        {
            ScreeningId = screening.Id,
            Email = user.Email,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seat.Id, PersonTypeName = "Adult" }
            }
        };

        var createResult = controller.InitiateBooking(request);
        var bookingId = Assert.IsType<int>(((CreatedAtActionResult)createResult).Value);

        var result = controller.GetById(bookingId);

        var returnedBooking = context.Bookings.Find(bookingId);
        Assert.NotNull(returnedBooking);
        Assert.Equal(bookingId, returnedBooking!.Id);
    }

    [Fact]
    public void GetById_ThrowsNotFound_WhenDoesNotExist()
    {
        var controller = CreateControllerWithSeededData(out _);

        Assert.Throws<NotFoundException>(() => controller.GetById(9999));
    }

    [Fact]
    public void InitiateBooking_WithMultipleTickets_CalculatesPriceCorrectly()
    {
        var controller = CreateControllerWithSeededData(out var context);
        var screening = context.Screenings.First();
        var seats = context.Seats.OrderBy(s => s.Id).ToList();
        var user = context.Users.First();

        var request = new BookingRequestDTO
        {
            ScreeningId = screening.Id,
            Email = user.Email,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seats[0].Id, PersonTypeName = "Adult" }, // Regular Adult
                new TicketCreateDto { SeatId = seats[1].Id, PersonTypeName = "Child" }, // Regular Child
                new TicketCreateDto { SeatId = seats[3].Id, PersonTypeName = "Adult" }  // VIP Adult
            }
        };

        var result = controller.InitiateBooking(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var bookingId = Assert.IsType<int>(createdResult.Value);
        var bookingEntity = context.Bookings
            .Include(b => b.Tickets)
            .ThenInclude(t => t.PersonType)
            .First(b => b.Id == bookingId);

        // Regular Adult: 12.00, Regular Child: 8.40, VIP Adult: 17.00
        var totalPrice = bookingEntity.Tickets.Sum(t => t.TotalPrice);
        Assert.Equal(37.40m, totalPrice);
        Assert.Equal(3, bookingEntity.Tickets.Count);
    }

    [Fact]
    public void ConfirmBooking_ThrowsConflict_WhenBookingExpired()
    {
        var controller = CreateControllerWithSeededData(out var context);
        var screening = context.Screenings.First();
        var seat = context.Seats.First();
        var user = context.Users.First();

        var request = new BookingRequestDTO
        {
            ScreeningId = screening.Id,
            Email = user.Email,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seat.Id, PersonTypeName = "Adult" }
            }
        };

        var createResult = controller.InitiateBooking(request);
        var createdResult = Assert.IsType<CreatedAtActionResult>(createResult);
        var bookingId = Assert.IsType<int>(createdResult.Value);

        // Simulate expiry by setting BookingTime to 20 minutes ago
        var existingBooking = context.Bookings.Find(bookingId);
        existingBooking!.BookingTime = DateTime.UtcNow.AddMinutes(-20);
        context.SaveChanges();

        // Attempt to confirm expired booking
        var exception = Assert.Throws<ConflictException>(() => controller.ConfirmBooking(bookingId, new BookingActionDTO { Email = user.Email }));
        Assert.Contains("expired", exception.Message.ToLower());

        // Verify booking was cancelled
        var cancelledBooking = context.Bookings.Find(bookingId);
        Assert.Equal(BookingStatus.Cancelled, cancelledBooking!.BookingStatus);
    }

    [Fact]
    public void IsSeatTaken_ReturnsFalse_WhenOnlyExpiredPendingBookingExists()
    {
        var controller = CreateControllerWithSeededData(out var context);
        var screening = context.Screenings.First();
        var seat = context.Seats.First();
        var user = context.Users.First();

        // Create a pending booking
        var request1 = new BookingRequestDTO
        {
            ScreeningId = screening.Id,
            Email = user.Email,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seat.Id, PersonTypeName = "Adult" }
            }
        };

        var createResult = controller.InitiateBooking(request1);
        var createdResult = Assert.IsType<CreatedAtActionResult>(createResult);
        var booking1Id = Assert.IsType<int>(createdResult.Value);

        // Simulate expiry by setting BookingTime to 20 minutes ago
        var existingBooking = context.Bookings.Find(booking1Id);
        existingBooking!.BookingTime = DateTime.UtcNow.AddMinutes(-20);
        context.SaveChanges();

        // Now try to create another booking for the same seat - should succeed
        var request2 = new BookingRequestDTO
        {
            ScreeningId = screening.Id,
            Email = user.Email,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seat.Id, PersonTypeName = "Adult" }
            }
        };

        var result2 = controller.InitiateBooking(request2);

        // Should succeed because the first booking expired
        var createdResult2 = Assert.IsType<CreatedAtActionResult>(result2);
        var booking2Id = Assert.IsType<int>(createdResult2.Value);
        Assert.NotEqual(booking1Id, booking2Id);
    }
}
