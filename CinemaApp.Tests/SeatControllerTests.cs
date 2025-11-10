using CinemaApp.Model;
using CinemaApp.Service;
using CinemaApp.Controller;
using CinemaApp.Data;
using CinemaApp.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using CinemaApp.Repository;

namespace CinemaApp.Tests;

public class SeatControllerTests
{
    private static SeatController CreateControllerWithSeededData(out AppDbContext context)
    {
        context = TestDataSeeder.CreateTestDbContext();
        TestDataSeeder.SeedLookupData(context);
        
        var (cinema, room, seats) = TestDataSeeder.SeedCinemaWithRoomAndSeats(context, regularSeats: 2, vipSeats: 0);

        ISeatService seatService = new SeatService(new SeatRepository(context), new RoomRepository(context));
        return new SeatController(seatService);
    }

    [Fact]
    public void GetByRoom_ReturnsSeatsForRoom()
    {
        var controller = CreateControllerWithSeededData(out var context);
        var roomId = 1;

        var result = controller.GetByRoom(roomId);

        var seats = Assert.IsType<List<Seat>>(result.Value);

        var seatsInContext = context.Seats.Where(s => s.RoomId == roomId).ToList();

        Assert.Equal(seatsInContext.Count, seats.Count);
        Assert.All(seats, s =>
        {
            Assert.Equal(roomId, s.RoomId);
            Assert.True(s.SeatTypeId > 0);
        });
    }

    [Fact]
    public void GetByRoom_ReturnsNotFound_WhenRoomDoesNotExist()
    {
        var controller = CreateControllerWithSeededData(out _);

        Assert.Throws<NotFoundException>(() => controller.GetByRoom(999));
    }

    [Fact]
    public void GenerateSeats_CreatesSeats_ForRoom()
    {
        var controller = CreateControllerWithSeededData(out var context);
        var seatTypeId = 1; // assuming seeded SeatType has Id = 1

        var room = context.Rooms.First();

        var result = controller.AddSeats(roomId: room.Id, rows: 2, seatsPerRow: 3, seatTypeId: seatTypeId);

        var okResult = Assert.IsType<CreatedAtActionResult>(result);
        var seats = Assert.IsType<List<Seat>>(okResult.Value);

        Assert.Equal(2 * 3, seats.Count);
        Assert.All(seats, s =>
        {
            Assert.Equal(room.Id, s.RoomId);
            Assert.Equal(seatTypeId, s.SeatTypeId);
        });
    }

    [Fact]
    public void GenerateSeats_ReturnsNotFound_WhenRoomDoesNotExist()
    {
        var controller = CreateControllerWithSeededData(out _);

        Assert.Throws<NotFoundException>(() => controller.AddSeats(999, rows: 1, seatsPerRow: 1, seatTypeId: 1));
    }

    [Fact]
    public void DeleteByRoom_RemovesAllSeatsInRoom()
    {
        var controller = CreateControllerWithSeededData(out var context);

        var room = context.Rooms.First();

        var result = controller.DeleteByRoom(room.Id);
        Assert.IsType<NoContentResult>(result);

        var seats = controller.GetByRoom(room.Id).Value!;
        Assert.Empty(seats);
    }

    [Fact]
    public void DeleteByRoom_ReturnsNotFound_WhenRoomDoesNotExist()
    {
        var controller = CreateControllerWithSeededData(out _);

        Assert.Throws<NotFoundException>(() => controller.DeleteByRoom(999));
    }
}
