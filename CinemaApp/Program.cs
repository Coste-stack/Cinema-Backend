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

// Configure DbContext with Oracle connection string from appsettings.json
services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleDb")));

services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
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
services.AddScoped<IBookingRepository, BookingRepository>();
services.AddScoped<ITicketRepository, TicketRepository>();

services.AddScoped<ILookupRepository<ProjectionType>, LookupRepository<ProjectionType>>();
services.AddScoped<ILookupRepository<SeatType>, LookupRepository<SeatType>>();
services.AddScoped<ILookupRepository<PersonType>, LookupRepository<PersonType>>();
services.AddScoped<ILookupRepository<Genre>, LookupRepository<Genre>>();

// Register services
services.AddScoped<ICinemaService, CinemaService>();
services.AddScoped<IRoomService, RoomService>();
services.AddScoped<ISeatService, SeatService>();
services.AddScoped<IMovieService, MovieService>();
services.AddScoped<IBookingService, BookingService>();
services.AddScoped<IPriceCalculationService, PriceCalculationService>();

services.AddScoped<ILookupService<ProjectionType>, LookupService<ProjectionType>>();
services.AddScoped<ILookupService<SeatType>, LookupService<SeatType>>();
services.AddScoped<ILookupService<PersonType>, LookupService<PersonType>>();
services.AddScoped<ILookupService<Genre>, LookupService<Genre>>();

services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

var app = builder.Build();

// --- Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.MapControllers();
app.Run();
