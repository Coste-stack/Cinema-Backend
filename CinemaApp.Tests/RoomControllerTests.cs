using CinemaApp.Model;
using CinemaApp.Repository;
using CinemaApp.Service;
using CinemaApp.Controller;
using CinemaApp.Data;

using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Xunit;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Tests;

public class RoomControllerTests
{
    private static AppDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static RoomController CreateControllerWithSeededData()
    {
        var context = CreateTestDbContext();

        // Create a cinema to link rooms to
        var cinema = new Cinema { Name = "Cinema Galaxy", Address = "Main Street 10", City = "Warsaw" };
        context.Cinemas.Add(cinema);
        context.SaveChanges();

        var room1 = new Room { Name = "Room A", Capacity = 150, CinemaId = cinema.Id };
        var room2 = new Room { Name = "Room B", Capacity = 200, CinemaId = cinema.Id };

        context.Rooms.AddRange(room1, room2);
        context.SaveChanges();

        IRoomRepository repository = new RoomRepository(context);
        IRoomService service = new RoomService(repository);
        ICinemaRepository cinemaRepo = new CinemaRepository(context);
        ICinemaService cinemaService = new CinemaService(cinemaRepo);

        // Note: RoomController constructor takes (IRoomService, ICinemaService)
        return new RoomController(service, cinemaService);
    }

    [Fact]
    public void GetAll_ReturnsAllRooms()
    {
        var controller = CreateControllerWithSeededData();

        var result = controller.GetAll();

        var rooms = Assert.IsType<List<Room>>(result.Value);
        Assert.True(rooms.Count >= 2);
        Assert.Contains(rooms, r => r.Name == "Room A");
    }

    [Fact]
    public void Get_ReturnsRoom_WhenExists()
    {
        var controller = CreateControllerWithSeededData();

        var result = controller.Get(1);

        var room = Assert.IsType<Room>(result.Value);
        Assert.Equal(1, room.Id);
        Assert.Equal("Room A", room.Name);
    }

    [Fact]
    public void Get_ReturnsNotFound_WhenDoesNotExist()
    {
        var controller = CreateControllerWithSeededData();

        var result = controller.Get(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void Create_AddsRoom_AndReturnsCreated_WithCinemaIdInBody()
    {
        var controller = CreateControllerWithSeededData();

        var room = new Room { Name = "Room C", Capacity = 120, CinemaId = 1 };

        var result = controller.Create(room);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var returnedRoom = Assert.IsType<Room>(created.Value);
        Assert.Equal("Room C", returnedRoom.Name);
        Assert.Equal(1, returnedRoom.CinemaId);
    }

    [Fact]
    public void Create_AddsRoom_UsingNestedEndpoint()
    {
        var controller = CreateControllerWithSeededData();

        var room = new Room { Name = "Room D", Capacity = 80 };

        var result = controller.Create(1, room);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var returnedRoom = Assert.IsType<Room>(created.Value);
        Assert.Equal("Room D", returnedRoom.Name);
        Assert.Equal(1, returnedRoom.CinemaId);
    }

    [Fact]
    public void Update_UpdatesRoom_WhenExists()
    {
        var controller = CreateControllerWithSeededData();

        var room = new Room { Id = 1, Name = "Room A+", Capacity = 180, CinemaId = 1 };

        var result = controller.Update(1, room);

        Assert.IsType<NoContentResult>(result);

        var updated = controller.Get(1).Value!;
        Assert.Equal("Room A+", updated.Name);
    }

    [Fact]
    public void Delete_RemovesRoom_WhenExists()
    {
        var controller = CreateControllerWithSeededData();

        var result = controller.Delete(1);

        Assert.IsType<NoContentResult>(result);

        var check = controller.Get(1).Result;
        Assert.IsType<NotFoundResult>(check);
    }

    [Fact]
    public void Delete_ReturnsNotFound_WhenDoesNotExist()
    {
        var controller = CreateControllerWithSeededData();

        var result = controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
