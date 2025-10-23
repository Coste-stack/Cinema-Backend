using CinemaApp.Data;
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

builder.Services.AddScoped<IProjectionTypeRepository, ProjectionTypeRepository>();

// Register services
builder.Services.AddScoped<ICinemaService, CinemaService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<IMovieService, MovieService>();

builder.Services.AddScoped<IProjectionTypeService, ProjectionTypeService>();

var app = builder.Build();

// --- Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
