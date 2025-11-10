using CinemaApp.Controller;
using CinemaApp.Data;
using CinemaApp.DTO.Ticket;
using CinemaApp.Model;
using CinemaApp.Repository;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Xunit;

namespace CinemaApp.Tests;

public class PriceControllerTests
{
    private static AppDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static PriceController CreateControllerWithSeededData(out int screeningId, out int seatId1, out int seatId2)
    {
        var context = CreateTestDbContext();

        // Seed lookup data
        var seatTypeRegular = new SeatType { Id = 1, Name = "Regular", PriceAmountDiscount = 0 };
        var seatTypeVIP = new SeatType { Id = 2, Name = "VIP", PriceAmountDiscount = 5 };
        var personTypeAdult = new PersonType { Id = 1, Name = "Adult", PricePercentDiscount = 0 };
        var personTypeChild = new PersonType { Id = 2, Name = "Child", PricePercentDiscount = 30 };
        var projectionType2D = new ProjectionType { Id = 1, Name = "2D", PriceAmountDiscount = 0 };

        context.SeatTypes.AddRange(seatTypeRegular, seatTypeVIP);
        context.PersonTypes.AddRange(personTypeAdult, personTypeChild);
        context.ProjectionTypes.Add(projectionType2D);
        context.SaveChanges();

        // Seed cinema, room, movie
        var cinema = new Cinema { Name = "Test Cinema", Address = "123 Test St", City = "Test City" };
        context.Cinemas.Add(cinema);
        context.SaveChanges();

        var room = new Room { Name = "Room 1", CinemaId = cinema.Id };
        context.Rooms.Add(room);
        context.SaveChanges();

        var movie = new Movie 
        { 
            Title = "Test Movie", 
            Duration = 120, 
            BasePrice = 10.00m 
        };
        context.Movies.Add(movie);
        context.SaveChanges();

        // Seed seats
        var seat1 = new Seat { Row = "A", Number = 1, RoomId = room.Id, SeatTypeId = seatTypeRegular.Id };
        var seat2 = new Seat { Row = "A", Number = 2, RoomId = room.Id, SeatTypeId = seatTypeVIP.Id };
        context.Seats.AddRange(seat1, seat2);
        context.SaveChanges();

        seatId1 = seat1.Id;
        seatId2 = seat2.Id;

        // Seed screening
        var screening = new Screening
        {
            MovieId = movie.Id,
            RoomId = room.Id,
            ProjectionTypeId = projectionType2D.Id,
            StartTime = DateTime.Now.AddDays(1),
            BasePrice = 12.00m
        };
        context.Screenings.Add(screening);
        context.SaveChanges();

        screeningId = screening.Id;

        // Create service and controller
        var bookingRepo = new BookingRepository(context);
        var screeningRepo = new ScreeningRepository(context);
        var priceService = new PriceCalculationService(bookingRepo, screeningRepo);
        return new PriceController(priceService);
    }

    [Fact]
    public void CalculatePrice_ReturnsCorrectPrice_WithDifferentSeatTypesAndPersonTypes()
    {
        var controller = CreateControllerWithSeededData(out int screeningId, out int seatId1, out int seatId2);

        // Test Regular Seat + Adult (BasePrice: 12, SeatType: 0, Discount: 0%) = 12.00
        var result1 = controller.CalculatePrice(screeningId, seatId1, personTypeId: 1);
        var okResult1 = Assert.IsType<OkObjectResult>(result1.Result);
        var value1 = okResult1.Value!;
        var price1 = (decimal)value1.GetType().GetProperty("price")!.GetValue(value1)!;
        Assert.Equal(12.00m, price1);

        // Test VIP Seat + Adult (BasePrice: 12, SeatType: +5, Discount: 0%) = 17.00
        var result2 = controller.CalculatePrice(screeningId, seatId2, personTypeId: 1);
        var okResult2 = Assert.IsType<OkObjectResult>(result2.Result);
        var value2 = okResult2.Value!;
        var price2 = (decimal)value2.GetType().GetProperty("price")!.GetValue(value2)!;
        Assert.Equal(17.00m, price2);

        // Test Regular Seat + Child (BasePrice: 12, SeatType: 0, Discount: 30%) = 8.40
        var result3 = controller.CalculatePrice(screeningId, seatId1, personTypeId: 2);
        var okResult3 = Assert.IsType<OkObjectResult>(result3.Result);
        var value3 = okResult3.Value!;
        var price3 = (decimal)value3.GetType().GetProperty("price")!.GetValue(value3)!;
        Assert.Equal(8.40m, price3);

        // Test VIP Seat + Child (BasePrice: 12, SeatType: +5, Discount: 30%) = 11.90
        var result4 = controller.CalculatePrice(screeningId, seatId2, personTypeId: 2);
        var okResult4 = Assert.IsType<OkObjectResult>(result4.Result);
        var value4 = okResult4.Value!;
        var price4 = (decimal)value4.GetType().GetProperty("price")!.GetValue(value4)!;
        Assert.Equal(11.90m, price4);
    }

    [Fact]
    public void CalculatePrice_ReturnsBadRequest_WhenIdsAreInvalid()
    {
        var controller = CreateControllerWithSeededData(out int screeningId, out int seatId1, out int seatId2);

        var result1 = controller.CalculatePrice(screeningId: 0, seatId: seatId1, personTypeId: 1);
        Assert.IsType<BadRequestObjectResult>(result1.Result);

        var result2 = controller.CalculatePrice(screeningId, seatId: -1, personTypeId: 1);
        Assert.IsType<BadRequestObjectResult>(result2.Result);

        var result3 = controller.CalculatePrice(screeningId, seatId: seatId1, personTypeId: 0);
        Assert.IsType<BadRequestObjectResult>(result3.Result);
    }

    [Fact]
    public void CalculateBulkPrice_ReturnsCorrectPricesAndTotal_ForMultipleTickets()
    {
        var controller = CreateControllerWithSeededData(out int screeningId, out int seatId1, out int seatId2);

        var request = new TicketBulkPriceRequestDTO
        {
            ScreeningId = screeningId,
            Tickets = new List<TicketPriceRequestDTO>
            {
                new TicketPriceRequestDTO { SeatId = seatId1, PersonTypeId = 1 }, // 12.00
                new TicketPriceRequestDTO { SeatId = seatId2, PersonTypeId = 2 }, // 11.90
                new TicketPriceRequestDTO { SeatId = seatId1, PersonTypeId = 2 }  // 8.40
            }
        };

        var result = controller.CalculateBulkPrice(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TicketBulkPriceResponseDTO>(okResult.Value);
        
        Assert.Equal(screeningId, response.ScreeningId);
        Assert.Equal(3, response.TicketPrices.Count);
        Assert.Equal(12.00m, response.TicketPrices[0].Price);
        Assert.Equal(11.90m, response.TicketPrices[1].Price);
        Assert.Equal(8.40m, response.TicketPrices[2].Price);
        Assert.Equal(32.30m, response.TotalPrice);
    }

    [Fact]
    public void CalculateBulkPrice_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        var controller = CreateControllerWithSeededData(out int screeningId, out int seatId1, out int seatId2);

        controller.ModelState.AddModelError("request", "Invalid request");
        
        var request = new TicketBulkPriceRequestDTO
        {
            ScreeningId = screeningId,
            Tickets = new List<TicketPriceRequestDTO>()
        };

        var result = controller.CalculateBulkPrice(request);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
