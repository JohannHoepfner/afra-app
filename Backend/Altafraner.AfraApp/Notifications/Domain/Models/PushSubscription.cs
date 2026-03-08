using System.ComponentModel.DataAnnotations;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Notifications.Domain.Models;

/// <summary>
///     A stored Web Push subscription for a specific user.
/// </summary>
public class PushSubscription
{
    /// <summary>
    ///     The unique identifier for this subscription.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The id of the subscribing person.
    /// </summary>
    public Guid PersonId { get; set; }

    /// <summary>
    ///     The subscribing person.
    /// </summary>
    public Person Person { get; set; } = null!;

    /// <summary>
    ///     The push service endpoint URL.
    /// </summary>
    [MaxLength(2048)]
    public required string Endpoint { get; set; }

    /// <summary>
    ///     The P256DH key for encrypting push messages.
    /// </summary>
    [MaxLength(512)]
    public required string P256dh { get; set; }

    /// <summary>
    ///     The auth secret for the push subscription.
    /// </summary>
    [MaxLength(256)]
    public required string Auth { get; set; }

    /// <summary>
    ///     When this subscription was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
