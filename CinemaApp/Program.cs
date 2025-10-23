using CinemaApp.Data;
using CinemaApp.Model;
using CinemaApp.Repository;
using CinemaApp.Service;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Add services to the container

builder.Services.AddOpenApi();

// Configure DbContext with Oracle connection string from appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleDb")));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });;

// Register repositories
builder.Services.AddScoped<ICinemaRepository, CinemaRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();

builder.Services.AddScoped<ILookupRepository<ProjectionType>, LookupRepository<ProjectionType>>();
builder.Services.AddScoped<ILookupRepository<SeatType>, LookupRepository<SeatType>>();
builder.Services.AddScoped<ILookupRepository<PersonType>, LookupRepository<PersonType>>();

// Register services
builder.Services.AddScoped<ICinemaService, CinemaService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<IMovieService, MovieService>();

builder.Services.AddScoped<ILookupService<ProjectionType>, LookupService<ProjectionType>>();
builder.Services.AddScoped<ILookupService<SeatType>, LookupService<SeatType>>();
builder.Services.AddScoped<ILookupService<PersonType>, LookupService<PersonType>>();

var app = builder.Build();

// --- Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
