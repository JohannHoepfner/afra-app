using System.Security.Claims;
using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Services;
using Altafraner.Backbone.CookieAuthentication;
using Altafraner.Backbone.OidcAuthentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;

namespace Altafraner.AfraApp.User.API.Endpoints;

/// <summary>
///     Extension Methods for the <see cref="UserSigninService" /> class.
/// </summary>
public static class User
{
    /// <summary>
    ///     Maps the user endpoints to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Local login (LDAP / DB, used when OIDC is disabled) ─────────────────
        app.MapPost("/api/user/login",
                async (UserSigninService userSigninService, UserSigninService.SignInRequest request,
                        IWebHostEnvironment environment) =>
                    await userSigninService.HandleSignInRequestAsync(request, environment))
            .WithName("sign-in")
            .AllowAnonymous();

        // ── OIDC login – redirects the browser to Keycloak ───────────────────────
        app.MapGet("/api/user/login",
                (HttpContext context, string? returnUrl,
                    IOptions<OidcAuthenticationSettings> oidcOptions) =>
                {
                    if (!oidcOptions.Value.Enabled)
                        return Results.NotFound();

                    var redirectUri = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
                    return Results.Challenge(
                        new AuthenticationProperties { RedirectUri = redirectUri },
                        [OpenIdConnectDefaults.AuthenticationScheme]);
                })
            .WithName("oidc-sign-in")
            .AllowAnonymous();

        // ── Current user info ────────────────────────────────────────────────────
        app.MapGet("/api/user",
            async (UserAccessor userAccessor, ClaimsPrincipal claimsPrincipal) =>
            {
                try
                {
                    var user = await userAccessor.GetUserAsync();
                    var impersonationId = claimsPrincipal.FindFirst(AfraAppClaimTypes.ImpersonatingUserId)?.Value;
                    return Results.Ok(new PersonLoginInfo
                    {
                        Id = user.Id,
                        Vorname = user.FirstName,
                        Nachname = user.LastName,
                        Rolle = user.Rolle,
                        Berechtigungen = user.GlobalPermissions.ToArray(),
                        ImpersonationId = impersonationId
                    });
                }
                catch (InvalidOperationException)
                {
                    return Results.Unauthorized();
                }
            });

        // ── Logout ───────────────────────────────────────────────────────────────
        // Always signs out of the local cookie.
        // When OIDC is enabled, also signs out of Keycloak (browser-redirect flow).
        app.MapGet("/api/user/logout",
                async (HttpContext context,
                    IAuthenticationLifetimeService authenticationLifetimeService,
                    IOptions<OidcAuthenticationSettings> oidcOptions) =>
                {
                    await authenticationLifetimeService.SignOutAsync();

                    if (oidcOptions.Value.Enabled)
                    {
                        // SignOutAsync with the OIDC scheme builds the end_session redirect.
                        // The handler writes a 302 to the response so we return immediately.
                        await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme,
                            new AuthenticationProperties { RedirectUri = "/" });
                        return Results.Empty;
                    }

                    return Results.Ok();
                })
            .RequireAuthorization();

        // ── Admin impersonation ──────────────────────────────────────────────────
        app.MapGet("/api/user/{id:guid}/impersonate",
                async (UserSigninService userSigninService,
                    ILogger<Program> logger,
                    Guid id,
                    UserAccessor userAccessor,
                    ClaimsPrincipal claimsPrincipal) =>
                {
                    var impersonationId = claimsPrincipal.FindFirst(AfraAppClaimTypes.ImpersonatingUserId)?.Value;
                    var currentUserId = impersonationId is not null
                        ? Guid.Parse(impersonationId)
                        : userAccessor.GetUserId();
                    logger.LogWarning("{oldUser} is Impersonating user with ID {newUser}", currentUserId, id);
                    await userSigninService.SignInAsync(id, false, currentUserId);
                })
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
    }

    /// <summary>
    ///     Maps the public auth-configuration endpoint used by the frontend to choose
    ///     between OIDC redirect and local login form.
    /// </summary>
    public static void MapAuthConfigEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/config",
                (IOptions<OidcAuthenticationSettings> oidcOptions) =>
                    Results.Ok(new { oidcEnabled = oidcOptions.Value.Enabled }))
            .WithName("auth-config")
            .AllowAnonymous();
    }
}
