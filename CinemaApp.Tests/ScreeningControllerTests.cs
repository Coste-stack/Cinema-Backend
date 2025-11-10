using System;
using System.Collections.Generic;
using System.Linq;
using CinemaApp.Controller;
using CinemaApp.Data;
using CinemaApp.Model;
using CinemaApp.Repository;
using CinemaApp.Service;
using CinemaApp.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CinemaApp.Tests
{
    public class ScreeningControllerTests
    {
        private static ScreeningController CreateControllerWithSeededData(out Movie movie, out Room room, out ProjectionType projectionType)
        {
            var context = TestDataSeeder.CreateTestDbContext();
            TestDataSeeder.SeedLookupData(context);

            var (cinema, roomSeeded) = TestDataSeeder.SeedCinemaWithRoom(context);
            room = roomSeeded;

            movie = TestDataSeeder.SeedMovie(context, title: "Inception", duration: 120);
            
            projectionType = context.ProjectionTypes.Find(1)!; // 2D from SeedLookupData

            // Set up dependencies
            ILookupRepository<ProjectionType> projectionTypeRepo = new LookupRepository<ProjectionType>(context);

            IScreeningRepository screeningRepo = new ScreeningRepository(context);
            IMovieRepository movieRepo = new MovieRepository(context);
            IRoomRepository roomRepo = new RoomRepository(context);

            IScreeningService screeningService = new ScreeningService(screeningRepo, movieRepo, roomRepo, projectionTypeRepo);

            return new ScreeningController(screeningService);
        }

        [Fact]
        public void GetAll_ReturnsEmpty_WhenNoScreenings()
        {
            var controller = CreateControllerWithSeededData(out _, out _, out _);

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
        public void Create_ThrowsNotFound_WhenProjectionTypeMissing()
        {
            var controller = CreateControllerWithSeededData(out var movie, out var room, out _);

            var screening = new Screening
            {
                MovieId = movie.Id,
                RoomId = room.Id,
                ProjectionTypeId = 999,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(2)
            };

            Assert.Throws<NotFoundException>(() => controller.Create(screening));
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
        public void GetById_ThrowsFound_WhenDoesNotExist()
        {
            var controller = CreateControllerWithSeededData(out _, out _, out _);

            Assert.Throws<NotFoundException>(() => controller.GetById(999));
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
        public void Update_ThrowsNotFound_WhenScreeningMissing()
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

            Assert.Throws<NotFoundException>(() => controller.Update(123, screening));
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

            // Create the screening
            var createResult = controller.Create(screening);
            var createdActionResult = Assert.IsType<CreatedAtActionResult>(createResult);
            var createdScreening = Assert.IsType<Screening>(createdActionResult.Value);

            // Delete the screening
            controller.DeleteById(createdScreening.Id);

            // Verify that fetching it throws NotFoundException
            Assert.Throws<NotFoundException>(() => controller.GetById(createdScreening.Id));
        }

        [Fact]
        public void DeleteByMovie_RemovesAllScreeningsForThatMovie()
        {
            var controller = CreateControllerWithSeededData(out var movie, out var room, out var projectionType);

            var s1 = new Screening { MovieId = movie.Id, RoomId = room.Id, ProjectionTypeId = projectionType.Id, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(2) };
            var s2 = new Screening { MovieId = movie.Id, RoomId = room.Id, ProjectionTypeId = projectionType.Id, StartTime = DateTime.UtcNow.AddHours(3), EndTime = DateTime.UtcNow.AddHours(5) };
            var createResult1 = controller.Create(s1);
            var createResult2 = controller.Create(s2);
            Assert.IsType<CreatedAtActionResult>(createResult1);
            Assert.IsType<CreatedAtActionResult>(createResult2);

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
