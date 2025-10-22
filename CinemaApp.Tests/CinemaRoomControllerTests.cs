using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CinemaApp.Model;
using CinemaApp.Data;
using CinemaApp.Repository;
using CinemaApp.Service;
using CinemaApp.Controller;

namespace CinemaApp.Tests
{
    public class CinemaRoomControllerTests
    {
        // Helper: create a fresh in-memory database for each test
        private static AppDbContext CreateTestDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        // Helper: seed data and create controllers
        private static (CinemaController cinemaController, RoomController roomController) CreateControllersWithSeedData()
        {
            var context = CreateTestDbContext();

            // Seed one cinema
            var cinema = new Cinema
            {
                Name = "Cinema Galaxy",
                Address = "Main Street 10",
                City = "Warsaw"
            };
            context.Cinemas.Add(cinema);
            context.SaveChanges();

            // Create repositories and services
            ICinemaRepository cinemaRepo = new CinemaRepository(context);
            IRoomRepository roomRepo = new RoomRepository(context);

            ICinemaService cinemaService = new CinemaService(cinemaRepo);
            IRoomService roomService = new RoomService(roomRepo);

            // Create controllers
            var cinemaController = new CinemaController(cinemaService);
            var roomController = new RoomController(roomService, cinemaService);

            return (cinemaController, roomController);
        }

        [Fact]
        public void AddRoomToExistingCinema_ShouldCreateRoomAndLinkCinema()
        {
            var (cinemaController, roomController) = CreateControllersWithSeedData();

            // Act
            var result = roomController.Create(1, new Room
            {
                Name = "Room 1",
                Capacity = 150
            });

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result);
            var room = Assert.IsType<Room>(created.Value);
            Assert.Equal(1, room.CinemaId);
            Assert.Equal("Room 1", room.Name);
        }

        [Fact]
        public void AddRoomToNonexistentCinema_ShouldReturnNotFound()
        {
            var (_, roomController) = CreateControllersWithSeedData();

            var result = roomController.Create(999, new Room
            {
                Name = "Invalid Room",
                Capacity = 100
            });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void GetCinema_ShouldIncludeRooms_WhenAdded()
        {
            var (cinemaController, roomController) = CreateControllersWithSeedData();

            // Add rooms to cinema 1
            roomController.Create(1, new Room { Name = "Room A", Capacity = 100 });
            roomController.Create(1, new Room { Name = "Room B", Capacity = 200 });

            // Retrieve cinema (ensure cinema service/Get returns Cinema with rooms loaded)
            var result = cinemaController.Get(1);
            var cinema = Assert.IsType<Cinema>(result.Value);

            Assert.NotNull(cinema.Rooms);
            Assert.True(cinema.Rooms.Count >= 2);
            Assert.Contains(cinema.Rooms, r => r.Name == "Room A");
            Assert.Contains(cinema.Rooms, r => r.Name == "Room B");
        }

        [Fact]
        public void Room_ShouldReference_CorrectCinema()
        {
            var (cinemaController, roomController) = CreateControllersWithSeedData();

            roomController.Create(1, new Room { Name = "Room X", Capacity = 150 });

            var roomResult = roomController.GetAll();
            var rooms = Assert.IsType<List<Room>>(roomResult.Value);
            var room = rooms.First();

            Assert.Equal(1, room.CinemaId);
        }

        [Fact]
        public void DeleteCinema_ShouldAlsoRemoveRooms()
        {
            var (cinemaController, roomController) = CreateControllersWithSeedData();

            roomController.Create(1, new Room { Name = "Room Y", Capacity = 120 });
            roomController.Create(1, new Room { Name = "Room Z", Capacity = 220 });

            var beforeDelete = roomController.GetAll().Value!;
            Assert.True(beforeDelete.Count >= 2);

            // Delete cinema
            cinemaController.Delete(1);

            var afterDelete = roomController.GetAll().Value!;
            Assert.Empty(afterDelete);
        }
    }
}
