using System.Net;
using Altafraner.AfraApp.IntegrationTests.Infrastructure;

namespace Altafraner.AfraApp.IntegrationTests.Tests;

/// <summary>
///     Integration tests for the Schuljahr (school year) endpoints (/api/schuljahr).
/// </summary>
public class SchuljahrEndpointTests : IClassFixture<AfraAppFactory>, IAsyncLifetime
{
    private readonly AfraAppFactory _factory;

    public SchuljahrEndpointTests(AfraAppFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync() => _factory.SeedDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetSchuljahr_WhenAuthenticated_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateStudentClient();

        // Act
        var response = await client.GetAsync("/api/schuljahr/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSchuljahr_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/schuljahr/");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetBlockSchemas_WhenAuthenticated_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateStudentClient();

        // Act
        var response = await client.GetAsync("/api/schuljahr/schemas");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBlocksForDate_WhenAuthenticated_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateStudentClient();
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Act
        var response = await client.GetAsync($"/api/schuljahr/{today:yyyy-MM-dd}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AddSchultage_AsTutor_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateTutorClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/management/schuljahr/",
            new[] { new { datum = "2025-05-05", wochentyp = "N", blocks = new[] { "1", "2" } } });

        // Assert - tutors without Otiumsverantwortlich permission should be forbidden
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetNow_WhenAuthenticated_ReturnsOkOrNotFound()
    {
        // Arrange
        var client = _factory.CreateStudentClient();

        // Act
        var response = await client.GetAsync("/api/schuljahr/now");

        // Assert - either there's a current block (OK) or not (NotFound)
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound);
    }
}
