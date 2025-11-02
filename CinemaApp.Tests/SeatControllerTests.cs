using CinemaApp.Model;
using CinemaApp.Service;
using CinemaApp.Controller;
using CinemaApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using CinemaApp.Repository;

namespace CinemaApp.Tests;

public class SeatControllerTests
{
    private static AppDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(System.Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static SeatController CreateControllerWithSeededData(out AppDbContext context)
    {
        context = CreateTestDbContext();

        // Seed a cinema and a room
        var cinema = new Cinema { Name = "Cinema Galaxy", Address = "Main Street 10", City = "Warsaw" };
        context.Cinemas.Add(cinema);
        context.SaveChanges();

        var room = new Room { Name = "Room 1", CinemaId = cinema.Id };
        context.Rooms.Add(room);
        context.SaveChanges();

        // Seed a SeatType
        var seatType = new SeatType { Name = "Regular" };
        context.SeatTypes.Add(seatType);
        context.SaveChanges();

        // Seed seats
        var seat1 = new Seat { RoomId = room.Id, Row = "A", Number = 1, SeatTypeId = seatType.Id };
        var seat2 = new Seat { RoomId = room.Id, Row = "A", Number = 2, SeatTypeId = seatType.Id };
        context.Seats.AddRange(seat1, seat2);
        context.SaveChanges();

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
