using CinemaApp.Controller;
using CinemaApp.Data;
using CinemaApp.Model;
using CinemaApp.Repository;
using CinemaApp.Service;
using CinemaApp.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CinemaApp.Tests;

public class AuthControllerTests
{
    private static AuthController CreateControllerWithSeededData(out AppDbContext context, out PasswordHasher<User> hasher, out ITokenService tokenService)
    {
        context = TestDataSeeder.CreateTestDbContext();
        hasher = new PasswordHasher<User>();
        TestDataSeeder.SeedUsersForAuthTests(context, hasher);

        var repo = new UserRepository(context);
        var service = new UserService(repo, hasher);
        tokenService = new TestTokenService();

        var inMemory = new Dictionary<string, string?> { { "Jwt:ExpiryMinutes", "60" } };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemory!)
            .Build();

        return new AuthController(service, hasher, tokenService, config);
    }

    [Fact]
    public void Login_ReturnsToken_WhenCredentialsValid()
    {
        var controller = CreateControllerWithSeededData(out var context, out var hasher, out var tokenService);

        var request = new LoginRequestDTO { Email = "reg@example.com", Password = "InitialPass1!" };

        var action = controller.Login(request);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var dto = Assert.IsType<LoginResponseDTO>(ok.Value);
        Assert.Equal(TestTokenService.TokenValue, dto.Token);
    }

    [Fact]
    public void Login_ReturnsUnauthorized_WhenPasswordInvalid()
    {
        var controller = CreateControllerWithSeededData(out var context, out var hasher, out var tokenService);

        var request = new LoginRequestDTO { Email = "reg@example.com", Password = "WrongPassword" };

        var action = controller.Login(request);

        Assert.IsType<UnauthorizedObjectResult>(action.Result);
    }

    [Fact]
    public void Register_ReturnsCreatedAndToken_WhenNewUser()
    {
        var context = TestDataSeeder.CreateTestDbContext();
        var hasher = new PasswordHasher<User>();
        var repo = new UserRepository(context);
        var service = new UserService(repo, hasher);
        var tokenService = new TestTokenService();
        var inMemory = new Dictionary<string, string?> { { "Jwt:ExpiryMinutes", "60" } };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemory!)
            .Build();

        var controller = new AuthController(service, hasher, tokenService, config);

        var dto = new UserCreateDTO { Email = "newuser@example.com", Password = "Secret!23" };

        var action = controller.Register(dto);

        var created = Assert.IsType<CreatedAtActionResult>(action.Result);
        var resp = Assert.IsType<LoginResponseDTO>(created.Value);
        Assert.Equal(TestTokenService.TokenValue, resp.Token);

        // user persisted
        var inDb = context.Users.SingleOrDefault(u => u.Email == dto.Email);
        Assert.NotNull(inDb);
    }

    private class TestTokenService : ITokenService
    {
        public const string TokenValue = "test-token-123";
        public string GenerateToken(User user) => TokenValue;
    }
}
