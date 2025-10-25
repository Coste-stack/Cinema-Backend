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

    private static CinemaController CreateControllerWithSeededData(out AppDbContext context)
    {
        context = CreateTestDbContext();

        var cinema1 = new Cinema { Name = "Cinema Galaxy", Address = "Main Street 10", City = "Warsaw" };
        var cinema2 = new Cinema { Name = "MegaFilm", Address = "Broadway 15", City = "Krakow" };

        context.Cinemas.AddRange(cinema1, cinema2);
        context.SaveChanges();

        var repository = new CinemaRepository(context);
        var service = new CinemaService(repository);
        var controller = new CinemaController(service);
        return controller;
    }

    [Fact]
    public void GetAll_ReturnsAllCinemas()
    {
        var controller = CreateControllerWithSeededData(out var context);

        var result = controller.GetAll();

        var cinemas = Assert.IsType<List<Cinema>>(result.Value);
        Assert.Equal(2, cinemas.Count);
        Assert.Contains(cinemas, c => c.Name == "Cinema Galaxy");
        Assert.Contains(cinemas, c => c.Name == "MegaFilm");
    }

    [Fact]
    public void GetById_ReturnsCinema_WhenExists()
    {
        var controller = CreateControllerWithSeededData(out var context);

        var existing = context.Cinemas.First();

        var result = controller.GetById(existing.Id);

        var cinema = Assert.IsType<Cinema>(result.Value);
        Assert.Equal(existing.Id, cinema.Id);
        Assert.Equal(existing.Name, cinema.Name);
    }

    [Fact]
    public void GetById_ReturnsNotFound_WhenDoesNotExist()
    {
        var controller = CreateControllerWithSeededData(out _);

        var result = controller.GetById(9999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void Create_AddsCinema_AndReturnsCreated()
    {
        var controller = CreateControllerWithSeededData(out _);

        var cinema = new Cinema { Name = "Nova Cinema", Address = "New Street 5", City = "Lodz" };

        var result = controller.Create(cinema);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var returned = Assert.IsType<Cinema>(createdResult.Value);
        Assert.Equal("Nova Cinema", returned.Name);
    }

    [Fact]
    public void Create_ReturnsBadRequest_WhenEmptyAddress()
    {
        var controller = CreateControllerWithSeededData(out _);

        var cinema = new Cinema { Name = "Nova Cinema", Address = "", City = "Lodz" };

        var result = controller.Create(cinema);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Update_UpdatesCinema_WhenExists()
    {
        var controller = CreateControllerWithSeededData(out var context);

        var existing = context.Cinemas.First();

        var updated = new Cinema
        {
            Name = "Cinema Galaxy Updated",
            Address = "Main Street 20",
            City = "Warsaw"
        };

        var result = controller.Update(existing.Id, updated);

        Assert.IsType<NoContentResult>(result);

        var refreshed = context.Cinemas.Find(existing.Id)!;
        Assert.Equal(existing.Id, refreshed.Id);
        Assert.Equal("Cinema Galaxy Updated", refreshed.Name);
        Assert.Equal("Main Street 20", refreshed.Address);
    }
}
