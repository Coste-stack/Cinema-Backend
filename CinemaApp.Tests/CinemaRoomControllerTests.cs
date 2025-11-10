using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using CinemaApp.Model;
using CinemaApp.Data;
using CinemaApp.Repository;
using CinemaApp.Service;
using CinemaApp.Controller;
using CinemaApp.Tests.Helpers;

namespace CinemaApp.Tests
{
    public class CinemaRoomControllerTests
    {
        // Helper: seed data and create controllers
        private static (CinemaController cinemaController, RoomController roomController) CreateControllersWithSeedData()
        {
            var context = TestDataSeeder.CreateTestDbContext();
            TestDataSeeder.SeedMultipleCinemas(context);

            // Create repositories and services
            ICinemaRepository cinemaRepo = new CinemaRepository(context);
            IRoomRepository roomRepo = new RoomRepository(context);

            ICinemaService cinemaService = new CinemaService(cinemaRepo);
            IRoomService roomService = new RoomService(roomRepo);

            // Create controllers
            var cinemaController = new CinemaController(cinemaService);
            var roomController = new RoomController(roomService);

            return (cinemaController, roomController);
        }

        [Fact]
        public void AddRoomToExistingCinema_ShouldCreateRoomAndLinkCinema()
        {
            var (_, roomController) = CreateControllersWithSeedData();

            // Act
            var result = roomController.Create(new Room
            {
                CinemaId = 1,
                Name = "Room 1"
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

            Assert.Throws<NotFoundException>(() => roomController.Create(new Room
            {
                CinemaId = 999,
                Name = "Invalid Room"
            }));
        }

        [Fact]
        public void GetCinema_ShouldIncludeRooms_WhenAdded()
        {
            var (cinemaController, roomController) = CreateControllersWithSeedData();

            // Add rooms to cinema 1
            roomController.Create(new Room { CinemaId = 1, Name = "Room A" });
            roomController.Create(new Room { CinemaId = 1, Name = "Room B" });

            // Retrieve cinema (ensure cinema service/Get returns Cinema with rooms loaded)
            var result = cinemaController.GetById(1);
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

            roomController.Create(new Room { CinemaId = 1, Name = "Room X" });

            var roomResult = roomController.GetAll();
            var rooms = Assert.IsType<List<Room>>(roomResult.Value);
            var room = rooms.First();

            Assert.Equal(1, room.CinemaId);
        }
    }
}
