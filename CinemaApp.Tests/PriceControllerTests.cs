using CinemaApp.Controller;
using CinemaApp.DTO.Ticket;
using CinemaApp.Model;
using CinemaApp.Repository;
using CinemaApp.Service;
using CinemaApp.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Xunit;

namespace CinemaApp.Tests;

public class PriceControllerTests
{
    private static PriceController CreateControllerWithSeededData(out int screeningId, out int regularSeatId, out int vipSeatId)
    {
        var context = TestDataSeeder.CreateTestDbContext();
        TestDataSeeder.SeedLookupData(context);

        var (cinema, room, seats) = TestDataSeeder.SeedCinemaWithRoomAndSeats(context, regularSeats: 2, vipSeats: 2);
        var movie = TestDataSeeder.SeedMovie(context, basePrice: 10.00m);
        var screening = TestDataSeeder.SeedScreening(context, movie.Id, room.Id, basePrice: 12.00m);

        regularSeatId = seats[0].Id; // First regular seat
        vipSeatId = seats[2].Id;     // First VIP seat
        screeningId = screening.Id;

        var bookingRepo = new BookingRepository(context);
        var screeningRepo = new ScreeningRepository(context);
        var personTypeRepo = new LookupRepository<PersonType>(context);
        var personTypeService = new LookupService<PersonType>(personTypeRepo);
        var priceService = new PriceCalculationService(bookingRepo, screeningRepo, personTypeService);
        
        return new PriceController(priceService);
    }

    [Fact]
    public void CalculatePrice_ReturnsCorrectPrice_WithDifferentSeatTypesAndPersonTypes()
    {
        var controller = CreateControllerWithSeededData(out int screeningId, out int seatId1, out int seatId2);

        // Test Regular Seat + Adult (BasePrice: 12, SeatType: 0, Discount: 0%) = 12.00
        var result1 = controller.CalculatePrice(screeningId, seatId1, personTypeName: "Adult");
        var okResult1 = Assert.IsType<OkObjectResult>(result1.Result);
        var value1 = okResult1.Value!;
        var price1 = (decimal)value1.GetType().GetProperty("price")!.GetValue(value1)!;
        Assert.Equal(12.00m, price1);

        // Test VIP Seat + Adult (BasePrice: 12, SeatType: +5, Discount: 0%) = 17.00
        var result2 = controller.CalculatePrice(screeningId, seatId2, personTypeName: "Adult");
        var okResult2 = Assert.IsType<OkObjectResult>(result2.Result);
        var value2 = okResult2.Value!;
        var price2 = (decimal)value2.GetType().GetProperty("price")!.GetValue(value2)!;
        Assert.Equal(17.00m, price2);

        // Test Regular Seat + Child (BasePrice: 12, SeatType: 0, Discount: 30%) = 8.40
        var result3 = controller.CalculatePrice(screeningId, seatId1, personTypeName: "Child");
        var okResult3 = Assert.IsType<OkObjectResult>(result3.Result);
        var value3 = okResult3.Value!;
        var price3 = (decimal)value3.GetType().GetProperty("price")!.GetValue(value3)!;
        Assert.Equal(8.40m, price3);

        // Test VIP Seat + Child (BasePrice: 12, SeatType: +5, Discount: 30%) = 11.90
        var result4 = controller.CalculatePrice(screeningId, seatId2, personTypeName: "Child");
        var okResult4 = Assert.IsType<OkObjectResult>(result4.Result);
        var value4 = okResult4.Value!;
        var price4 = (decimal)value4.GetType().GetProperty("price")!.GetValue(value4)!;
        Assert.Equal(11.90m, price4);
    }

    [Fact]
    public void CalculatePrice_ThrowsBadRequest_WhenIdsAreInvalid()
    {
        var controller = CreateControllerWithSeededData(out int screeningId, out int seatId1, out int seatId2);

        Assert.Throws<BadRequestException>(() => controller.CalculatePrice(screeningId: 0, seatId: seatId1, personTypeName: "Adult"));
        Assert.Throws<BadRequestException>(() => controller.CalculatePrice(screeningId, seatId: -1, personTypeName: "Adult"));
        Assert.Throws<BadRequestException>(() => controller.CalculatePrice(screeningId, seatId: seatId1, personTypeName: ""));
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
                new TicketPriceRequestDTO { SeatId = seatId1, PersonTypeName = "Adult" }, // 12.00
                new TicketPriceRequestDTO { SeatId = seatId2, PersonTypeName = "Child" }, // 11.90
                new TicketPriceRequestDTO { SeatId = seatId1, PersonTypeName = "Child" }  // 8.40
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
