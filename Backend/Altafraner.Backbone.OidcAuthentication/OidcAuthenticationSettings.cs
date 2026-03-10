namespace Altafraner.Backbone.OidcAuthentication;

/// <summary>
///     Settings for Keycloak / OIDC authentication.
/// </summary>
public class OidcAuthenticationSettings
{
    /// <summary>
    ///     Whether OIDC authentication is enabled.
    ///     When <c>false</c> the application falls back to the local (LDAP / database) login form.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    ///     The OIDC authority URL, e.g. <c>https://keycloak.example.com/realms/myrealm</c>.
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    ///     The OIDC client ID registered in Keycloak.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    ///     The OIDC client secret registered in Keycloak.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    ///     The path the OIDC provider redirects to after a successful login.
    ///     Defaults to <c>/signin-oidc</c>.
    /// </summary>
    public string CallbackPath { get; set; } = "/signin-oidc";

    /// <summary>
    ///     The path the OIDC provider redirects to after a successful logout.
    ///     Defaults to <c>/signout-callback-oidc</c>.
    /// </summary>
    public string SignedOutCallbackPath { get; set; } = "/signout-callback-oidc";

    /// <summary>
    ///     The claim used to identify the user in the local database.
    ///     The lookup is tried against the user's <c>Email</c> field first, then against the beginning of the
    ///     <c>Email</c> field (username part).
    ///     Defaults to <c>email</c>.
    /// </summary>
    public string UserIdentifierClaim { get; set; } = "email";
}
