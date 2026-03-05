using System.Net;
using System.Net.Http.Json;
using Altafraner.AfraApp.IntegrationTests.Infrastructure;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.IntegrationTests.Tests;

/// <summary>
///     Integration tests for the user endpoints (/api/user).
/// </summary>
public class UserEndpointTests : IClassFixture<AfraAppFactory>, IAsyncLifetime
{
    private readonly AfraAppFactory _factory;

    public UserEndpointTests(AfraAppFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync() => _factory.SeedDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetUser_WhenAuthenticated_ReturnsUserInfo()
    {
        // Arrange
        var client = _factory.CreateClientAs(AfraAppFactory.TutorUserId, Rolle.Tutor);

        // Act
        var response = await client.GetAsync("/api/user");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<UserInfoResponse>();
        Assert.NotNull(body);
        Assert.Equal(AfraAppFactory.TutorUserId, body.Id);
    }

    [Fact]
    public async Task GetUser_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/user");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/user/login",
            new { Username = "tutor@example.com", Password = "any", RememberMe = false });

        // Assert
        // In development mode without LDAP, login checks by email prefix matching - any password works
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/user/login",
            new { Username = "doesnotexist@example.com", Password = "wrongpassword", RememberMe = false });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetPeople_AsTutor_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateTutorClient();

        // Act
        var response = await client.GetAsync("/api/people");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPeople_AsStudent_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateStudentClient();

        // Act
        var response = await client.GetAsync("/api/people");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetKlassen_WhenAuthenticated_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateStudentClient();

        // Act
        var response = await client.GetAsync("/api/klassen");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private record UserInfoResponse(Guid Id, string Vorname, string Nachname, Rolle Rolle, GlobalPermission[] Berechtigungen);
}
