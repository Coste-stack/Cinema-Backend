using CinemaApp.Data;
using CinemaApp.Model;
using CinemaApp.Repository;
using CinemaApp.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Add services to the container

var services = builder.Services;

services.AddOpenApi();

// Configure CORS
services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure DbContext with Postgresql
var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

if (isDocker)
{
    // Docker secrets path or fallback to env var
    var passwordPath = "/run/secrets/db-password";
    string? password = null;

    if (File.Exists(passwordPath))
    {
        password = File.ReadAllText(passwordPath).Trim();
    }
    else
    {
        // Common environment variables used in docker-compose/Postgres setups
        password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")
                   ?? Environment.GetEnvironmentVariable("DB_PASSWORD");
    }

    if (string.IsNullOrWhiteSpace(password))
        throw new Exception("Database password not provided. Mount secret to '/run/secrets/db-password' or set 'POSTGRES_PASSWORD' or 'DB_PASSWORD' environment variable.");

    var dockerConn = $"Host=db;Port=5432;Database=CinemaAppDb;Username=postgres;Password={password}";
    builder.Configuration["ConnectionStrings:PostgresDb"] = dockerConn;
}

// // Register DbContext
var connectionString = builder.Configuration.GetConnectionString("PostgresDb");
services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// var connectionString = builder.Configuration.GetConnectionString("default");
// services.AddDbContext<AppDbContext>(options =>
//     options.UseNpgsql(connectionString));

services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });;

// Register exception handlers
services.AddExceptionHandler<BadRequestExceptionHandler>();
services.AddExceptionHandler<NotFoundExceptionHandler>();
services.AddExceptionHandler<ConflictExceptionHandler>();
services.AddExceptionHandler<GlobalExceptionHandler>();
services.AddProblemDetails();

// Register repositories
services.AddScoped<ICinemaRepository, CinemaRepository>();
services.AddScoped<IRoomRepository, RoomRepository>();
services.AddScoped<ISeatRepository, SeatRepository>();
services.AddScoped<IMovieRepository, MovieRepository>();
services.AddScoped<IScreeningRepository, ScreeningRepository>();
services.AddScoped<IBookingRepository, BookingRepository>();
services.AddScoped<ITicketRepository, TicketRepository>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IStatisticsRepository, StatisticsRepository>();

services.AddScoped<ILookupRepository<ProjectionType>, LookupRepository<ProjectionType>>();
services.AddScoped<ILookupRepository<SeatType>, LookupRepository<SeatType>>();
services.AddScoped<ILookupRepository<PersonType>, LookupRepository<PersonType>>();
services.AddScoped<ILookupRepository<Genre>, LookupRepository<Genre>>();

// Register services
services.AddScoped<ICinemaService, CinemaService>();
services.AddScoped<IRoomService, RoomService>();
services.AddScoped<ISeatService, SeatService>();
services.AddScoped<IMovieService, MovieService>();
services.AddScoped<IScreeningService, ScreeningService>();
services.AddScoped<IBookingService, BookingService>();
services.AddScoped<IUserService, UserService>();
services.AddScoped<IPriceCalculationService, PriceCalculationService>();
services.AddScoped<IStatisticsService, StatisticsService>();

services.AddScoped<ILookupService<ProjectionType>, LookupService<ProjectionType>>();
services.AddScoped<ILookupService<SeatType>, LookupService<SeatType>>();
services.AddScoped<ILookupService<PersonType>, LookupService<PersonType>>();
services.AddScoped<ILookupService<Genre>, LookupService<Genre>>();

services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Register background services
services.AddHostedService<BookingExpiryBackgroundService>();

var app = builder.Build();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    db.Database.EnsureCreated(); // creates DB & tables if they don't exist
    await SeedData.InitializeAsync(context, logger);
}

// --- Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");
app.UseExceptionHandler();
app.MapControllers();
app.Run();
