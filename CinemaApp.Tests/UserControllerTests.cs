using CinemaApp.Model;
using CinemaApp.Service;
using CinemaApp.Controller;
using CinemaApp.Data;
using CinemaApp.Repository;
using CinemaApp.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace CinemaApp.Tests;

public class UserControllerTests
{
    private static UserController CreateControllerWithSeededData(out AppDbContext context, out PasswordHasher<User> passwordHasher)
    {
        context = TestDataSeeder.CreateTestDbContext();
        passwordHasher = new PasswordHasher<User>();
        
        TestDataSeeder.SeedUsersForAuthTests(context, passwordHasher);

        var repo = new UserRepository(context);
        var service = new UserService(repo, passwordHasher);
        return new UserController(service);
    }

    [Fact]
    public void GetAll_ReturnsAllUsers_AsResponseDtos()
    {
        var controller = CreateControllerWithSeededData(out var context, out _);

        var result = controller.GetAll();

        var users = Assert.IsType<List<UserResponseDTO>>(result.Value);
        Assert.Equal(2, users.Count);
        Assert.Contains(users, u => u.Email == "guest@example.com");
        Assert.Contains(users, u => u.Email == "reg@example.com");
    }

    [Fact]
    public void GetById_ReturnsUserResponseDto_WhenExists()
    {
        var controller = CreateControllerWithSeededData(out var context, out _);
        var existing = context.Users.First();

        var result = controller.GetById(existing.Id);

        var userDto = Assert.IsType<UserResponseDTO>(result.Value);
        Assert.Equal(existing.Email, userDto.Email);
        Assert.Equal(existing.Id, userDto.Id);
    }

    [Fact]
    public void GetById_ReturnsNotFound_WhenNotExists()
    {
        var controller = CreateControllerWithSeededData(out _, out _);

        Assert.Throws<NotFoundException>(() => controller.GetById(9999));
    }

    [Fact]
    public void GetByEmail_ReturnsUserResponseDto_WhenExists()
    {
        var controller = CreateControllerWithSeededData(out var context, out _);
        var existing = context.Users.First();

        var result = controller.GetByEmail(existing.Email);

        var userDto = Assert.IsType<UserResponseDTO>(result.Value);
        Assert.Equal(existing.Email, userDto.Email);
        Assert.Equal(existing.Id, userDto.Id);
    }


    [Fact]
    public void GetByEmail_ReturnsNotFound_WhenNotExists()
    {
        var controller = CreateControllerWithSeededData(out _, out _);

        Assert.Throws<NotFoundException>(() => controller.GetByEmail("aaabbb@example.com"));
    }

    [Fact]
    public void Create_CreatesRegisteredUser_WhenPasswordProvided_ReturnsResponseDto()
    {
        var controller = CreateControllerWithSeededData(out var context, out var hasher);

        var dto = new UserCreateDTO
        {
            Email = "newuser@example.com",
            Password = "Secret!23"
        };

        var action = controller.Register(dto);

        var created = Assert.IsType<CreatedAtActionResult>(action);
        var createdDto = Assert.IsType<UserResponseDTO>(created.Value);

        Assert.Equal(dto.Email, createdDto.Email);
        Assert.Equal(UserType.Registered, createdDto.UserType);
        Assert.True(createdDto.Id > 0);
        Assert.NotEqual(default, createdDto.CreatedAt);

        // persisted and password hashed in DB
        var inDb = context.Users.SingleOrDefault(u => u.Email == dto.Email);
        Assert.NotNull(inDb);
        Assert.False(string.IsNullOrEmpty(inDb!.PasswordHash));
        var verification = hasher.VerifyHashedPassword(inDb, inDb.PasswordHash!, dto.Password);
        Assert.Equal(PasswordVerificationResult.Success, verification);
    }

    [Fact]
    public void Create_CreatesGuestUser_WhenNoPasswordProvided_ReturnsResponseDto()
    {
        var controller = CreateControllerWithSeededData(out var context, out _);

        var dto = new UserCreateDTO
        {
            Email = "guest2@example.com",
            Password = null
        };

        var action = controller.Register(dto);

        var created = Assert.IsType<CreatedAtActionResult>(action);
        var createdDto = Assert.IsType<UserResponseDTO>(created.Value);

        Assert.Equal(dto.Email, createdDto.Email);
        Assert.Equal(UserType.Guest, createdDto.UserType);

        var inDb = context.Users.SingleOrDefault(u => u.Email == dto.Email);
        Assert.NotNull(inDb);
        Assert.Null(inDb!.PasswordHash);
    }

    [Fact]
    public void Create_ThrowsBadRequest_WhenEmailAlreadyExists()
    {
        var controller = CreateControllerWithSeededData(out var context, out _);

        var dto = new UserCreateDTO
        {
            Email = "guest@example.com",
            Password = "Anything"
        };

        Assert.Throws<BadRequestException>(() => controller.Register(dto));
    }

    [Fact]
    public void Update_UpdatesEmailAndPassword_WhenValid()
    {
        var controller = CreateControllerWithSeededData(out var context, out var hasher);

        var user = context.Users.First(u => u.Email == "guest@example.com");

        var dto = new UserCreateDTO
        {
            Email = "updated@example.com",
            Password = "NewPassword!1"
        };

        var action = controller.Update(user.Id, dto);
        Assert.IsType<NoContentResult>(action);

        var updated = context.Users.Find(user.Id)!;
        Assert.Equal("updated@example.com", updated.Email);
        Assert.Equal(UserType.Registered, updated.UserType);
        Assert.NotNull(updated.PasswordHash);

        var verification = hasher.VerifyHashedPassword(updated, updated.PasswordHash!, dto.Password);
        Assert.Equal(PasswordVerificationResult.Success, verification);
    }

    [Fact]
    public void Update_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var controller = CreateControllerWithSeededData(out _, out _);

        var dto = new UserCreateDTO
        {
            Email = "noone@example.com",
            Password = "pass"
        };

        Assert.Throws<NotFoundException>(() => controller.Update(9999, dto));
    }

    [Fact]
    public void Update_ReturnsBadRequest_WhenEmailIsUsedByAnotherUser()
    {
        var controller = CreateControllerWithSeededData(out var context, out _);

        var userToUpdate = context.Users.First(u => u.Email == "guest@example.com");
        var other = context.Users.First(u => u.Email == "reg@example.com");

        var dto = new UserCreateDTO
        {
            Email = other.Email, // collision
            Password = null
        };

        Assert.Throws<BadRequestException>(() => controller.Update(userToUpdate.Id, dto));
    }
}