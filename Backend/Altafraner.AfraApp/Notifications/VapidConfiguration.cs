namespace Altafraner.AfraApp.Notifications;

/// <summary>
///     Configuration for Web Push (VAPID) notifications.
/// </summary>
public class VapidConfiguration
{
    /// <summary>
    ///     The VAPID public key (URL-safe base64 encoded).
    /// </summary>
    public required string PublicKey { get; set; }

    /// <summary>
    ///     The VAPID private key (URL-safe base64 encoded).
    /// </summary>
    public required string PrivateKey { get; set; }

    /// <summary>
    ///     The VAPID subject (e.g. mailto: or URL).
    /// </summary>
    public required string Subject { get; set; }

    internal static bool Validate(VapidConfiguration config)
    {
        return !string.IsNullOrWhiteSpace(config.PublicKey)
               && !string.IsNullOrWhiteSpace(config.PrivateKey)
               && !string.IsNullOrWhiteSpace(config.Subject);
    }
}
