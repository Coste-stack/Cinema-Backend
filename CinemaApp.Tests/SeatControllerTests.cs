using CinemaApp.Model;
using CinemaApp.Service;
using CinemaApp.Controller;
using CinemaApp.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using System.Collections.Generic;
using System.Linq;

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

    private static SeatController CreateControllerWithSeededData()
    {
        var context = CreateTestDbContext();

        // Seed a room
        var cinema = new Cinema { Name = "Cinema Galaxy", Address = "Main Street 10", City = "Warsaw" };
        context.Cinemas.Add(cinema);
        context.SaveChanges();

        var room = new Room { Name = "Room 1", CinemaId = cinema.Id };
        context.Rooms.Add(room);
        context.SaveChanges();

        // Seed seats
        var seat1 = new Seat { RoomId = room.Id, Row = "A", Number = 1 };
        var seat2 = new Seat { RoomId = room.Id, Row = "A", Number = 2 };
        context.Seats.AddRange(seat1, seat2);
        context.SaveChanges();

        ISeatService seatService = new SeatService(new Repository.SeatRepository(context));
        IRoomService roomService = new RoomService(new Repository.RoomRepository(context));

        return new SeatController(seatService, roomService);
    }

    [Fact]
    public void GetByRoom_ReturnsSeatsForRoom()
    {
        var controller = CreateControllerWithSeededData();

        var roomId = 1;
        var result = controller.GetByRoom(roomId);

        var seats = Assert.IsType<List<Seat>>(result.Value);
        Assert.Equal(2, seats.Count);
        Assert.All(seats, s => Assert.Equal(roomId, s.RoomId));
    }

    [Fact]
    public void GetByRoom_ReturnsNotFound_WhenRoomDoesNotExist()
    {
        var controller = CreateControllerWithSeededData();

        var result = controller.GetByRoom(999);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void GenerateSeats_CreatesSeats_ForRoom()
    {
        var controller = CreateControllerWithSeededData();

        var result = controller.GenerateSeats(1, rows: 2, seatsPerRow: 3);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var seats = Assert.IsType<List<Seat>>(okResult.Value);

        Assert.Equal(2 * 3, seats.Count);
        Assert.All(seats, s => Assert.Equal(1, s.RoomId));
    }

    [Fact]
    public void GenerateSeats_ReturnsNotFound_WhenRoomDoesNotExist()
    {
        var controller = CreateControllerWithSeededData();

        var result = controller.GenerateSeats(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void Delete_RemovesSeat_WhenExists()
    {
        var controller = CreateControllerWithSeededData();

        var result = controller.Delete(1);
        Assert.IsType<NoContentResult>(result);

        var check = controller.GetByRoom(1).Value!;
        Assert.DoesNotContain(check, s => s.Id == 1);
    }

    [Fact]
    public void Delete_ReturnsNotFound_WhenSeatDoesNotExist()
    {
        var controller = CreateControllerWithSeededData();

        var result = controller.Delete(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void DeleteByRoom_RemovesAllSeatsInRoom()
    {
        var controller = CreateControllerWithSeededData();

        var result = controller.DeleteByRoom(1);
        Assert.IsType<NoContentResult>(result);

        var seats = controller.GetByRoom(1).Value!;
        Assert.Empty(seats);
    }

    [Fact]
    public void DeleteByRoom_ReturnsNotFound_WhenRoomDoesNotExist()
    {
        var controller = CreateControllerWithSeededData();

        var result = controller.DeleteByRoom(999);
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
