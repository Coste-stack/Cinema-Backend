using System;
using System.Collections.Generic;
using System.Linq;
using CinemaApp.Controller;
using CinemaApp.Data;
using CinemaApp.Model;
using CinemaApp.Repository;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CinemaApp.Tests;

public class ScreeningControllerTests
{
    private static AppDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static ScreeningController CreateControllerWithSeededData(out Movie movie, out Room room)
    {
        var context = CreateTestDbContext();

        // Seed movie and room
        movie = new Movie { Title = "Inception", Duration = 120, Genre = "Sci-Fi" };
        room = new Room { Name = "Room 1", Capacity = 100 };
        context.Movies.Add(movie);
        context.Rooms.Add(room);
        context.SaveChanges();

        // Set up dependencies
        IScreeningRepository screeningRepo = new ScreeningRepository(context);
        IScreeningService screeningService = new ScreeningService(screeningRepo);

        IMovieRepository movieRepo = new MovieRepository(context);
        IMovieService movieService = new MovieService(movieRepo);

        IRoomRepository roomRepo = new RoomRepository(context);
        IRoomService roomService = new RoomService(roomRepo);

        return new ScreeningController(screeningService, movieService, roomService);
    }

    [Fact]
    public void GetAll_ReturnsEmpty_WhenNoScreenings()
    {
        var controller = CreateControllerWithSeededData(out var movie, out var room);

        var result = controller.GetAll();

        var screenings = Assert.IsType<List<Screening>>(result.Value);
        Assert.Empty(screenings);
    }

    [Fact]
    public void Create_AddsScreening_AndReturnsCreated()
    {
        var controller = CreateControllerWithSeededData(out var movie, out var room);

        var screening = new Screening
        {
            MovieId = movie.Id,
            RoomId = room.Id,
            StartTime = DateTime.Now,
            Price = 30.0f
        };

        var result = controller.Create(screening);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var returnedScreening = Assert.IsType<Screening>(created.Value);

        Assert.Equal(screening.MovieId, returnedScreening.MovieId);
        Assert.NotNull(returnedScreening.EndTime);
    }

    [Fact]
    public void Create_ReturnsNotFound_WhenMovieMissing()
    {
        var controller = CreateControllerWithSeededData(out var movie, out var room);

        var screening = new Screening
        {
            MovieId = 999,
            RoomId = room.Id,
            StartTime = DateTime.Now,
            Price = 25
        };

        var result = controller.Create(screening);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Movie with ID 999 not found", notFound.Value?.ToString() ?? string.Empty);
    }

    [Fact]
    public void GetById_ReturnsScreening_WhenExists()
    {
        var controller = CreateControllerWithSeededData(out var movie, out var room);

        var screening = new Screening
        {
            MovieId = movie.Id,
            RoomId = room.Id,
            StartTime = DateTime.Now,
            Price = 40
        };

        controller.Create(screening);

        var result = controller.GetById(screening.Id);

        var found = Assert.IsType<Screening>(result.Value);
        Assert.Equal(screening.Id, found.Id);
    }

    [Fact]
    public void GetById_ReturnsNotFound_WhenDoesNotExist()
    {
        var controller = CreateControllerWithSeededData(out var movie, out var room);

        var result = controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void Update_ModifiesExistingScreening()
    {
        var controller = CreateControllerWithSeededData(out var movie, out var room);

        var screening = new Screening
        {
            MovieId = movie.Id,
            RoomId = room.Id,
            StartTime = DateTime.Now,
            Price = 50
        };
        controller.Create(screening);

        screening.Price = 60;
        var result = controller.Update(screening.Id, screening);

        Assert.IsType<NoContentResult>(result);

        var updated = controller.GetById(screening.Id).Value!;
        Assert.Equal(60, updated.Price);
    }

    [Fact]
    public void Update_ReturnsNotFound_WhenScreeningMissing()
    {
        var controller = CreateControllerWithSeededData(out var movie, out var room);

        var screening = new Screening
        {
            Id = 123,
            MovieId = movie.Id,
            RoomId = room.Id,
            StartTime = DateTime.Now,
            Price = 25
        };

        var result = controller.Update(123, screening);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void DeleteById_RemovesScreening()
    {
        var controller = CreateControllerWithSeededData(out var movie, out var room);

        var screening = new Screening
        {
            MovieId = movie.Id,
            RoomId = room.Id,
            StartTime = DateTime.Now,
            Price = 45
        };
        controller.Create(screening);

        var deleteResult = controller.DeleteById(screening.Id);
        Assert.IsType<NoContentResult>(deleteResult);

        var check = controller.GetById(screening.Id).Result;
        Assert.IsType<NotFoundResult>(check);
    }

    [Fact]
    public void DeleteByMovie_RemovesAllScreeningsForThatMovie()
    {
        var controller = CreateControllerWithSeededData(out var movie, out var room);

        var s1 = new Screening { MovieId = movie.Id, RoomId = room.Id, StartTime = DateTime.Now, Price = 20 };
        var s2 = new Screening { MovieId = movie.Id, RoomId = room.Id, StartTime = DateTime.Now.AddHours(3), Price = 20 };
        controller.Create(s1);
        controller.Create(s2);

        var result = controller.DeleteByMovie(movie.Id);

        Assert.IsType<NoContentResult>(result);

        var remaining = controller.GetByMovie(movie.Id).Value!;
        Assert.Empty(remaining);
    }

    [Fact]
    public void Create_ThrowsError_WhenOverlapping()
    {
        var controller = CreateControllerWithSeededData(out var movie, out var room);

        var s1 = new Screening
        {
            MovieId = movie.Id,
            RoomId = room.Id,
            StartTime = DateTime.Now,
            Price = 20
        };
        controller.Create(s1);

        var s2 = new Screening
        {
            MovieId = movie.Id,
            RoomId = room.Id,
            StartTime = s1.StartTime.AddMinutes(30),
            Price = 20
        };

        var ex = Assert.Throws<InvalidOperationException>(() => controller.Create(s2));
        Assert.Contains("Room is already booked", ex.Message);
    }
}
