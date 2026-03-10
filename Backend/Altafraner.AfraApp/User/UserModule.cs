using System.Security.Claims;
using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.User.API.Endpoints;
using Altafraner.AfraApp.User.Configuration.LDAP;
using Altafraner.AfraApp.User.Services;
using Altafraner.AfraApp.User.Services.LDAP;
using Altafraner.Backbone.Abstractions;
using Altafraner.Backbone.OidcAuthentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Altafraner.AfraApp.User;

/// <summary>
///     A module for handling users
/// </summary>
[DependsOn<DatabaseModule>]
[DependsOn<AuthorizationModule>]
[DependsOn<OidcAuthenticationModule>]
public class UserModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddOptions<LdapConfiguration>()
            .Bind(config.GetSection("LDAP"))
            .Validate(LdapConfiguration.Validate)
            .ValidateOnStart();

        services.AddScoped<UserSigninService>();
        services.AddScoped<UserAccessor>();
        services.AddScoped<UserService>();
        services.AddScoped<UserAuthorizationHelper>();
        services.AddHttpContextAccessor();
        services.AddScoped<LdapService>();

        services.AddHostedService<LdapAutoSyncScheduler>();

        // When OIDC is enabled, hook into the token-validation event to map the Keycloak identity
        // to a local AfraApp user and replace the claims principal with AfraApp-specific claims.
        var oidcSettings = config.GetSection("Oidc").Get<OidcAuthenticationSettings>()
                           ?? new OidcAuthenticationSettings();
        if (!oidcSettings.Enabled) return;

        services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.Events.OnTokenValidated = async ctx =>
            {
                if (ctx.Principal is null)
                {
                    ctx.Fail("OIDC principal was null after token validation.");
                    return;
                }

                var db = ctx.HttpContext.RequestServices.GetRequiredService<AfraAppContext>();
                var settings = ctx.HttpContext.RequestServices
                    .GetRequiredService<IOptions<OidcAuthenticationSettings>>().Value;

                var identifierValue = ctx.Principal.FindFirstValue(settings.UserIdentifierClaim)
                                      ?? ctx.Principal.FindFirstValue("email")
                                      ?? ctx.Principal.FindFirstValue("preferred_username");

                if (string.IsNullOrWhiteSpace(identifierValue))
                {
                    ctx.Fail(
                        $"OIDC claim '{settings.UserIdentifierClaim}' was missing or empty. " +
                        "Cannot identify local user.");
                    return;
                }

                // Look up the user by full e-mail match first, then by username prefix.
                var person = await db.Personen.FirstOrDefaultAsync(u => u.Email == identifierValue)
                             ?? await db.Personen.FirstOrDefaultAsync(
                                 u => u.Email.StartsWith(identifierValue + "@"));

                if (person is null)
                {
                    ctx.Fail(
                        $"No local user found for OIDC identifier '{identifierValue}'. " +
                        "Make sure the user has been synced from LDAP.");
                    return;
                }

                // Replace the Keycloak claims with AfraApp-specific claims so that the resulting
                // cookie contains only what the application needs.
                var afraAppClaims = UserSigninService.GenerateClaims(person);
                var identity = new ClaimsIdentity(afraAppClaims,
                    CookieAuthenticationDefaults.AuthenticationScheme);
                ctx.Principal = new ClaimsPrincipal(identity);
            };
        });
    }

    /// <inheritdoc />
    public void Configure(WebApplication app)
    {
        app.MapUserEndpoints();
        app.MapPeopleEndpoints();
        app.MapAuthConfigEndpoint();
    }
}
