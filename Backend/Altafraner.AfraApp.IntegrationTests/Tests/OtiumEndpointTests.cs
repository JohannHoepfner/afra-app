using System.Net;
using Altafraner.AfraApp.IntegrationTests.Infrastructure;

namespace Altafraner.AfraApp.IntegrationTests.Tests;

/// <summary>
///     Integration tests for the Otium endpoints (/api/otium).
/// </summary>
public class OtiumEndpointTests : IClassFixture<AfraAppFactory>, IAsyncLifetime
{
    private readonly AfraAppFactory _factory;

    public OtiumEndpointTests(AfraAppFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync() => _factory.SeedDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetKategorien_WhenAuthenticated_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateStudentClient();

        // Act
        var response = await client.GetAsync("/api/otium/kategorie");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetKategorien_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/otium/kategorie");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetKatalogForDay_WhenAuthenticatedAsStudent_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateStudentClient();
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Act
        var response = await client.GetAsync($"/api/otium/{today:yyyy-MM-dd}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetStudentDashboard_WhenAuthenticatedAsStudent_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateStudentClient();

        // Act
        var response = await client.GetAsync("/api/otium/student");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetStudentDashboard_WhenAuthenticatedAsTutor_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateTutorClient();

        // Act
        var response = await client.GetAsync("/api/otium/student");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetTeacherDashboard_WhenAuthenticatedAsTutor_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateTutorClient();

        // Act
        var response = await client.GetAsync("/api/otium/teacher");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTeacherDashboard_WhenAuthenticatedAsStudent_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateStudentClient();

        // Act
        var response = await client.GetAsync("/api/otium/teacher");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetManagementOtia_WhenAuthenticatedAsTutor_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateTutorClient();

        // Act
        var response = await client.GetAsync("/api/otium/management/otium");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetManagementOtia_WhenAuthenticatedAsStudent_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateStudentClient();

        // Act
        var response = await client.GetAsync("/api/otium/management/otium");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
