using System.Security.Claims;
using System.Text.Encodings.Web;
using Altafraner.AfraApp.Backbone.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altafraner.AfraApp.IntegrationTests.Infrastructure;

/// <summary>
///     A test authentication handler that authenticates requests based on special test headers.
///     Set <see cref="UserIdHeader" /> and <see cref="RoleHeader" /> on the HTTP client to authenticate.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";
    public const string UserIdHeader = "X-Test-UserId";
    public const string RoleHeader = "X-Test-Role";
    public const string PermissionHeader = "X-Test-Permission";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserIdHeader, out var userIdValues) ||
            !Request.Headers.TryGetValue(RoleHeader, out var roleValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userId = userIdValues.ToString();
        var role = roleValues.ToString();

        var claims = new List<Claim>
        {
            new(AfraAppClaimTypes.Id, userId),
            new(AfraAppClaimTypes.Role, role),
            new(AfraAppClaimTypes.GivenName, "Test"),
            new(AfraAppClaimTypes.LastName, "User"),
        };

        if (Request.Headers.TryGetValue(PermissionHeader, out var permissions))
        {
            claims.AddRange(permissions.Select(p => new Claim(AfraAppClaimTypes.GlobalPermission, p!)));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
