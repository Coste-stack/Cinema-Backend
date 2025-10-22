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

public class CinemaControllerTests
{
    private static AppDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static CinemaController CreateControllerWithSeededData()
    {
        var context = CreateTestDbContext();

        var cinema1 = new Cinema { Name = "Cinema Galaxy", Address = "Main Street 10", City = "Warsaw" };
        var cinema2 = new Cinema { Name = "MegaFilm", Address = "Broadway 15", City = "Krakow" };

        context.Cinemas.AddRange(cinema1, cinema2);
        context.SaveChanges();

        ICinemaRepository repository = new CinemaRepository(context);
        ICinemaService service = new CinemaService(repository);
        return new CinemaController(service);
    }

    [Fact]
    public void GetAll_ReturnsAllCinemas()
    {
        var controller = CreateControllerWithSeededData();

        var result = controller.GetAll();

        var cinemas = Assert.IsType<List<Cinema>>(result.Value);
        Assert.True(cinemas.Count >= 2);
        Assert.Contains(cinemas, c => c.Name == "Cinema Galaxy");
    }

    [Fact]
    public void Get_ReturnsCinema_WhenExists()
    {
        var controller = CreateControllerWithSeededData();

        var result = controller.Get(1);

        var cinema = Assert.IsType<Cinema>(result.Value);
        Assert.Equal(1, cinema.Id);
        Assert.Equal("Cinema Galaxy", cinema.Name);
    }

    [Fact]
    public void Get_ReturnsNotFound_WhenDoesNotExist()
    {
        var controller = CreateControllerWithSeededData();

        var result = controller.Get(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void Create_AddsCinema_AndReturnsCreated()
    {
        var controller = CreateControllerWithSeededData();

        var cinema = new Cinema { Name = "Nova Cinema", Address = "New Street 5", City = "Lodz" };

        var result = controller.Create(cinema);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var returned = Assert.IsType<Cinema>(createdResult.Value);
        Assert.Equal("Nova Cinema", returned.Name);
    }

    [Fact]
    public void Update_UpdatesCinema_WhenExists()
    {
        var controller = CreateControllerWithSeededData();

        var updated = new Cinema
        {
            Id = 1,
            Name = "Cinema Galaxy Updated",
            Address = "Main Street 20",
            City = "Warsaw"
        };

        var result = controller.Update(1, updated);

        Assert.IsType<NoContentResult>(result);

        var refreshed = controller.Get(1).Value!;
        Assert.Equal("Cinema Galaxy Updated", refreshed.Name);
    }

    [Fact]
    public void Delete_RemovesCinema_WhenExists()
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
