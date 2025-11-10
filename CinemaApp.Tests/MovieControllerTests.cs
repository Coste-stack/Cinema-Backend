using CinemaApp.Model;
using CinemaApp.Repository;
using CinemaApp.Service;
using CinemaApp.Controller;
using CinemaApp.Data;
using CinemaApp.Tests.Helpers;
using System;
using System.Collections.Generic;
using Xunit;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Tests;

public class MovieControllerTests
{
    private static MovieController CreateControllerWithSeededData()
    {
        var context = TestDataSeeder.CreateTestDbContext();
        TestDataSeeder.SeedMoviesWithGenres(context);

        IMovieRepository repository = new MovieRepository(context);
        IMovieService service = new MovieService(repository);
        return new MovieController(service);
    }

    [Fact]
    public void Test_DbContext_Creation()
    {
        var context = TestDataSeeder.CreateTestDbContext();
        Assert.NotNull(context);
    }

    [Fact]
    public void GetAll_ReturnsAllMovies()
    {
        var controller = CreateControllerWithSeededData();

        var result = controller.GetAll();

        var movies = Assert.IsType<List<Movie>>(result.Value);
        Assert.True(movies.Count >= 2);
        Assert.Contains(movies, m => m.Title == "Inception");
        Assert.Contains(movies, m => m.Title == "The Dark Knight");
    }

    [Fact]
    public void Get_ReturnsMovie_WhenExists()
    {
        var controller = CreateControllerWithSeededData();

        var result = controller.GetById(1);

        var movie = Assert.IsType<Movie>(result.Value);
        Assert.Equal(1, movie.Id);
        Assert.Equal("Inception", movie.Title);
    }

    [Fact]
    public void Get_ReturnsNotFound_WhenDoesNotExist()
    {
        var controller = CreateControllerWithSeededData();

        Assert.Throws<NotFoundException>(() => controller.GetById(9999));
    }

    [Fact]
    public void Create_AddsMovieAndReturnsCreated()
    {
        var context = TestDataSeeder.CreateTestDbContext();
        var genreSciFi = TestDataSeeder.SeedGenre(context, "Sci-Fi");

        IMovieRepository repository = new MovieRepository(context);
        IMovieService service = new MovieService(repository);
        var controller = new MovieController(service);

        var movie = new Movie
        {
            Title = "Interstellar",
            Description = "Exploration of space and time.",
            Duration = 169,
            Rating = MovieRating.PG13,
            ReleaseDate = new DateTime(2014, 11, 7),
            Genres = new List<Genre> { genreSciFi }
        };

        var result = controller.Create(movie);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var returnedMovie = Assert.IsType<Movie>(createdResult.Value);
        Assert.Equal("Interstellar", returnedMovie.Title);
        Assert.Equal(MovieRating.PG13, returnedMovie.Rating);
    }

    [Fact]
    public void Update_ReturnsBadRequest_WhenIdMismatch()
    {
        var controller = CreateControllerWithSeededData();
        var movie = new Movie
        {
            Id = 1,
            Title = "Inception Updated",
            Duration = 150
        };

        Assert.Throws<BadRequestException>(() => controller.Update(2, movie));
    }

    [Fact]
    public void Update_ReturnsNotFound_WhenMovieDoesNotExist()
    {
        var controller = CreateControllerWithSeededData();
        var movie = new Movie
        {
            Id = 999,
            Title = "Nonexistent Movie",
            Duration = 120
        };

        Assert.Throws<NotFoundException>(() => controller.Update(999, movie));
    }

    [Fact]
    public void Update_UpdatesMovie_WhenExists()
    {
        var context = TestDataSeeder.CreateTestDbContext();
        
        var genreSciFi = TestDataSeeder.SeedGenre(context, "Sci-Fi");
        var genreAction = TestDataSeeder.SeedGenre(context, "Action");
        
        // Seed a movie with genre
        var existingMovie = new Movie
        {
            Title = "Inception",
            Duration = 148,
            Genres = new List<Genre> { genreSciFi }
        };
        context.Movies.Add(existingMovie);
        context.SaveChanges();

        IMovieRepository repository = new MovieRepository(context);
        IMovieService service = new MovieService(repository);
        var controller = new MovieController(service);

        var movie = new Movie
        {
            Id = existingMovie.Id,
            Title = "Inception Reloaded",
            Duration = 155,
            Genres = new List<Genre> { genreAction }
        };

        var result = controller.Update(existingMovie.Id, movie);

        Assert.IsType<NoContentResult>(result);

        var updated = controller.GetById(existingMovie.Id).Value!;
        Assert.Equal("Inception Reloaded", updated.Title);
        Assert.Equal(155, updated.Duration);
    }

    [Fact]
    public void Delete_RemovesMovie_WhenExists()
    {
        var controller = CreateControllerWithSeededData();
        var movies = controller.GetAll().Value!;
        var lastMovie = movies[movies.Count - 1];

        var result = controller.Delete(lastMovie.Id);

        Assert.IsType<NoContentResult>(result);

        Assert.Throws<NotFoundException>(() => controller.GetById(lastMovie.Id).Result);
    }

    [Fact]
    public void Delete_ReturnsNotFound_WhenDoesNotExist()
    {
        var controller = CreateControllerWithSeededData();

        Assert.Throws<NotFoundException>(() => controller.Delete(9999));
    }
}
