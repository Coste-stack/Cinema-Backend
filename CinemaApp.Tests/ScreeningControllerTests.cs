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

namespace CinemaApp.Tests
{
    public class ScreeningControllerTests
    {
        private static AppDbContext CreateTestDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static ScreeningController CreateControllerWithSeededData(out Movie movie, out Room room, out ProjectionType projectionType)
        {
            var context = CreateTestDbContext();

            // Seed movie, room, projection type
            movie = new Movie { Title = "Inception", Duration = 120, Genre = "Sci-Fi" };
            room = new Room { Name = "Room 1" };
            projectionType = new ProjectionType { Name = "2D" };

            context.Movies.Add(movie);
            context.Rooms.Add(room);
            context.ProjectionTypes.Add(projectionType);
            context.SaveChanges();

            // Set up dependencies
            IScreeningRepository screeningRepo = new ScreeningRepository(context);
            IScreeningService screeningService = new ScreeningService(screeningRepo);

            IMovieRepository movieRepo = new MovieRepository(context);
            IMovieService movieService = new MovieService(movieRepo);

            IRoomRepository roomRepo = new RoomRepository(context);
            IRoomService roomService = new RoomService(roomRepo);

            ILookupRepository<ProjectionType> projectionTypeRepo = new LookupRepository<ProjectionType>(context);
            ILookupService<ProjectionType> projectionTypeService = new LookupService<ProjectionType>(projectionTypeRepo);

            return new ScreeningController(screeningService, movieService, roomService, projectionTypeService);
        }

        [Fact]
        public void GetAll_ReturnsEmpty_WhenNoScreenings()
        {
            var controller = CreateControllerWithSeededData(out var movie, out var room, out var projectionType);

            var result = controller.GetAll();

            var screenings = Assert.IsType<List<Screening>>(result.Value);
            Assert.Empty(screenings);
        }

        [Fact]
        public void Create_AddsScreening_AndReturnsCreated()
        {
            var controller = CreateControllerWithSeededData(out var movie, out var room, out var projectionType);

            var screening = new Screening
            {
                MovieId = movie.Id,
                RoomId = room.Id,
                ProjectionTypeId = projectionType.Id,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(2)
            };

            var result = controller.Create(screening);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            var returnedScreening = Assert.IsType<Screening>(created.Value);

            Assert.Equal(screening.MovieId, returnedScreening.MovieId);
            Assert.Equal(projectionType.Id, returnedScreening.ProjectionTypeId);
            Assert.Equal(screening.EndTime, returnedScreening.EndTime);
        }

        [Fact]
        public void Create_ReturnsNotFound_WhenMovieMissing()
        {
            var controller = CreateControllerWithSeededData(out var movie, out var room, out var projectionType);

            var screening = new Screening
            {
                MovieId = 999,
                RoomId = room.Id,
                ProjectionTypeId = projectionType.Id,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(2)
            };

            var result = controller.Create(screening);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Movie with ID 999 not found", notFound.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public void Create_ReturnsNotFound_WhenProjectionTypeMissing()
        {
            var controller = CreateControllerWithSeededData(out var movie, out var room, out var projectionType);

            var screening = new Screening
            {
                MovieId = movie.Id,
                RoomId = room.Id,
                ProjectionTypeId = 999,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(2)
            };

            var result = controller.Create(screening);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("ProjectionType with ID 999 not found", notFound.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public void GetById_ReturnsScreening_WhenExists()
        {
            var controller = CreateControllerWithSeededData(out var movie, out var room, out var projectionType);

            var screening = new Screening
            {
                MovieId = movie.Id,
                RoomId = room.Id,
                ProjectionTypeId = projectionType.Id,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(2)
            };

            controller.Create(screening);

            var result = controller.GetById(screening.Id);

            var found = Assert.IsType<Screening>(result.Value);
            Assert.Equal(screening.Id, found.Id);
            Assert.Equal(projectionType.Id, found.ProjectionTypeId);
        }

        [Fact]
        public void GetById_ReturnsNotFound_WhenDoesNotExist()
        {
            var controller = CreateControllerWithSeededData(out var movie, out var room, out var projectionType);

            var result = controller.GetById(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void Update_ModifiesExistingScreening()
        {
            var controller = CreateControllerWithSeededData(out var movie, out var room, out var projectionType);

            var screening = new Screening
            {
                MovieId = movie.Id,
                RoomId = room.Id,
                ProjectionTypeId = projectionType.Id,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(2)
            };
            controller.Create(screening);

            screening.StartTime = screening.StartTime.AddHours(2);
            var result = controller.Update(screening.Id, screening);

            Assert.IsType<NoContentResult>(result);

            var updated = controller.GetById(screening.Id).Value!;
            Assert.Equal(screening.StartTime, updated.StartTime);
            Assert.Equal(projectionType.Id, updated.ProjectionTypeId);
        }

        [Fact]
        public void Update_ReturnsNotFound_WhenScreeningMissing()
        {
            var controller = CreateControllerWithSeededData(out var movie, out var room, out var projectionType);

            var screening = new Screening
            {
                Id = 123,
                MovieId = movie.Id,
                RoomId = room.Id,
                ProjectionTypeId = projectionType.Id,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(2)
            };

            var result = controller.Update(123, screening);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeleteById_RemovesScreening()
        {
            var controller = CreateControllerWithSeededData(out var movie, out var room, out var projectionType);

            var screening = new Screening
            {
                MovieId = movie.Id,
                RoomId = room.Id,
                ProjectionTypeId = projectionType.Id,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(2)
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
            var controller = CreateControllerWithSeededData(out var movie, out var room, out var projectionType);

            var s1 = new Screening { MovieId = movie.Id, RoomId = room.Id, ProjectionTypeId = projectionType.Id, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(2) };
            var s2 = new Screening { MovieId = movie.Id, RoomId = room.Id, ProjectionTypeId = projectionType.Id, StartTime = DateTime.UtcNow.AddHours(3), EndTime = DateTime.UtcNow.AddHours(5) };
            controller.Create(s1);
            controller.Create(s2);

            var result = controller.DeleteByMovie(movie.Id);

            Assert.IsType<NoContentResult>(result);

            var remaining = controller.GetByMovie(movie.Id).Value!;
            Assert.Empty(remaining);
        }

        [Fact]
        public void DeleteByRoom_RemovesAllScreeningsForThatRoom()
        {
            var controller = CreateControllerWithSeededData(out var movie, out var room, out var projectionType);

            var s1 = new Screening { MovieId = movie.Id, RoomId = room.Id, ProjectionTypeId = projectionType.Id, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(2) };
            var s2 = new Screening { MovieId = movie.Id, RoomId = room.Id, ProjectionTypeId = projectionType.Id, StartTime = DateTime.UtcNow.AddHours(3), EndTime = DateTime.UtcNow.AddHours(5) };
            controller.Create(s1);
            controller.Create(s2);

            var result = controller.DeleteByRoom(room.Id);

            Assert.IsType<NoContentResult>(result);

            var remaining = controller.GetByRoom(room.Id).Value!;
            Assert.Empty(remaining);
        }

        [Fact]
        public void Create_SetsEndTimeBasedOnMovieDuration()
        {
            var controller = CreateControllerWithSeededData(out var movie, out var room, out var projectionType);

            var start = DateTime.UtcNow;
            var screening = new Screening
            {
                MovieId = movie.Id,
                RoomId = room.Id,
                ProjectionTypeId = projectionType.Id,
                StartTime = start,
                EndTime = null
            };

            var result = controller.Create(screening);
            var created = Assert.IsType<CreatedAtActionResult>(result);
            var returned = Assert.IsType<Screening>(created.Value);

            Assert.Equal(start.AddMinutes(movie.Duration), returned.EndTime);
        }

        [Fact]
        public void Update_PreservesExistingEndTime_WhenNewEndTimeIsNull()
        {
            var controller = CreateControllerWithSeededData(out var movie, out var room, out var projectionType);

            var screening = new Screening
            {
                MovieId = movie.Id,
                RoomId = room.Id,
                ProjectionTypeId = projectionType.Id,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(2),
                Language = "English"
            };
            controller.Create(screening);

            var updated = new Screening
            {
                Id = screening.Id,
                MovieId = movie.Id,
                RoomId = room.Id,
                ProjectionTypeId = projectionType.Id,
                StartTime = screening.StartTime.AddHours(1),
                EndTime = null,
                Language = null
            };
            controller.Update(screening.Id, updated);

            var result = controller.GetById(screening.Id).Value!;
            Assert.Equal(screening.EndTime, result.EndTime);
            Assert.Equal("English", result.Language);
        }
    }
}
