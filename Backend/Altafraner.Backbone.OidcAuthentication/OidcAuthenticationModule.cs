using Altafraner.Backbone.Abstractions;
using Altafraner.Backbone.CookieAuthentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Altafraner.Backbone.OidcAuthentication;

/// <summary>
///     A module that adds Keycloak / OIDC authentication on top of the existing cookie session.
///     When enabled (<see cref="OidcAuthenticationSettings.Enabled" /> is <c>true</c>), OIDC becomes
///     the default challenge scheme while the cookie scheme continues to manage local sessions.
/// </summary>
[DependsOn<CookieAuthenticationModule>]
public class OidcAuthenticationModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        var section = config.GetSection("Oidc");
        services.AddOptions<OidcAuthenticationSettings>().Bind(section);

        var settings = section.Exists()
            ? section.Get<OidcAuthenticationSettings>() ?? new OidcAuthenticationSettings()
            : new OidcAuthenticationSettings();

        if (!settings.Enabled) return;

        // Make OIDC the challenge scheme so unauthenticated requests are redirected to Keycloak.
        services.Configure<AuthenticationOptions>(options =>
        {
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        });

        services.AddAuthentication()
            .AddOpenIdConnect(options =>
            {
                options.Authority = settings.Authority;
                options.ClientId = settings.ClientId;
                options.ClientSecret = settings.ClientSecret;
                options.CallbackPath = settings.CallbackPath;
                options.SignedOutCallbackPath = settings.SignedOutCallbackPath;

                // Authorization-code flow – safest choice for server-side apps.
                options.ResponseType = "code";

                // Tokens are only needed during the callback to populate local claims.
                // They are not persisted in the cookie.
                options.SaveTokens = false;
                options.GetClaimsFromUserInfoEndpoint = true;

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
            });
    }
}
