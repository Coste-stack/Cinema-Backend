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
        var priceService = new PriceCalculationService(bookingRepo, screeningRepo);
        var bookingService = new BookingService(bookingRepo, ticketRepo, priceService);

        return new BookingController(bookingService);
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
            UserId = user.Id,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seat.Id, PersonTypeId = 1 } // Adult
            }
        };

        var result = controller.InitiateBooking(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var booking = Assert.IsType<Booking>(createdResult.Value);
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
            UserId = user.Id,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seats[0].Id, PersonTypeId = 1 }, // Adult, Regular seat
                new TicketCreateDto { SeatId = seats[1].Id, PersonTypeId = 2 }  // Child, Regular seat
            }
        };

        var result = controller.InitiateBooking(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var booking = Assert.IsType<Booking>(createdResult.Value);
        
        // Adult: 12.00, Child: 8.40 (30% discount)
        var totalPrice = booking.Tickets.Sum(t => t.TotalPrice);
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
            UserId = user.Id,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seat.Id, PersonTypeId = 1 }
            }
        };
        controller.InitiateBooking(request1);

        // Try to book the same seat again
        var request2 = new BookingRequestDTO
        {
            ScreeningId = screening.Id,
            UserId = user.Id,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seat.Id, PersonTypeId = 1 }
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
            UserId = user.Id,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seat.Id, PersonTypeId = 1 }
            }
        };

        var createResult = controller.InitiateBooking(request);
        var createdResult = Assert.IsType<CreatedAtActionResult>(createResult);
        var booking = Assert.IsType<Booking>(createdResult.Value);

        var confirmResult = controller.ConfirmBooking(booking.Id);

        Assert.IsType<NoContentResult>(confirmResult);

        var confirmed = controller.GetById(booking.Id).Value;
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
            UserId = user.Id,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seat.Id, PersonTypeId = 1 }
            }
        };

        var createResult = controller.InitiateBooking(request);
        var createdResult = Assert.IsType<CreatedAtActionResult>(createResult);
        var booking = Assert.IsType<Booking>(createdResult.Value);

        var cancelResult = controller.CancelBooking(booking.Id);

        Assert.IsType<NoContentResult>(cancelResult);

        var cancelled = controller.GetById(booking.Id).Value;
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
            UserId = user.Id,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seat.Id, PersonTypeId = 1 }
            }
        };
        var createResult1 = controller.InitiateBooking(request1);
        var booking1 = Assert.IsType<Booking>(((CreatedAtActionResult)createResult1).Value);

        // Cancel it
        controller.CancelBooking(booking1.Id);

        // Book the same seat again - should succeed
        var request2 = new BookingRequestDTO
        {
            ScreeningId = screening.Id,
            UserId = user.Id,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seat.Id, PersonTypeId = 1 }
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
            UserId = user.Id,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seats[0].Id, PersonTypeId = 1 }
            }
        };
        controller.InitiateBooking(request1);

        var request2 = new BookingRequestDTO
        {
            ScreeningId = screening.Id,
            UserId = user.Id,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seats[1].Id, PersonTypeId = 1 }
            }
        };
        controller.InitiateBooking(request2);

        var result = controller.GetMyBookings(user.Id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var bookings = Assert.IsType<List<Booking>>(okResult.Value);
        Assert.Equal(2, bookings.Count);
        Assert.All(bookings, b => Assert.Equal(user.Id, b.UserId));
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
            UserId = user.Id,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seat.Id, PersonTypeId = 1 }
            }
        };

        var createResult = controller.InitiateBooking(request);
        var booking = Assert.IsType<Booking>(((CreatedAtActionResult)createResult).Value);

        var result = controller.GetById(booking.Id);

        var returnedBooking = Assert.IsType<Booking>(result.Value);
        Assert.Equal(booking.Id, returnedBooking.Id);
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
            UserId = user.Id,
            Tickets = new List<TicketCreateDto>
            {
                new TicketCreateDto { SeatId = seats[0].Id, PersonTypeId = 1 }, // Regular Adult
                new TicketCreateDto { SeatId = seats[1].Id, PersonTypeId = 2 }, // Regular Child
                new TicketCreateDto { SeatId = seats[3].Id, PersonTypeId = 1 }  // VIP Adult
            }
        };

        var result = controller.InitiateBooking(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var booking = Assert.IsType<Booking>(createdResult.Value);
        
        // Regular Adult: 12.00, Regular Child: 8.40, VIP Adult: 17.00
        var totalPrice = booking.Tickets.Sum(t => t.TotalPrice);
        Assert.Equal(37.40m, totalPrice);
        Assert.Equal(3, booking.Tickets.Count);
    }
}
