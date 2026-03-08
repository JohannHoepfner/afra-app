using System.Text.Json;
using Altafraner.AfraApp.Notifications.API.Hubs;
using Altafraner.AfraApp.Notifications.Domain.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using AppPushSubscription = Altafraner.AfraApp.Notifications.Domain.Models.PushSubscription;

namespace Altafraner.AfraApp.Notifications.Services;

/// <summary>
///     Stores in-app notifications in the database, delivers them in real time via SignalR,
///     and sends Web Push messages when the user has active push subscriptions.
/// </summary>
internal class InAppNotificationService : IInAppNotificationService
{
    private readonly AfraAppContext _db;
    private readonly IHubContext<NotificationHub, INotificationHubClient> _hub;
    private readonly ILogger<InAppNotificationService> _logger;
    private readonly WebPushSender _webPushSender;

    /// <summary>
    ///     Constructs a new <see cref="InAppNotificationService" />.
    /// </summary>
    public InAppNotificationService(
        AfraAppContext db,
        IHubContext<NotificationHub, INotificationHubClient> hub,
        ILogger<InAppNotificationService> logger,
        WebPushSender webPushSender)
    {
        _db = db;
        _hub = hub;
        _logger = logger;
        _webPushSender = webPushSender;
    }

    /// <inheritdoc />
    public async Task SendInAppNotificationAsync(Guid recipientId, string subject, string body)
    {
        var notification = new InAppNotification
        {
            Id = Guid.CreateVersion7(),
            RecipientId = recipientId,
            Subject = subject,
            Body = body,
            CreatedAt = DateTime.UtcNow
        };

        _db.InAppNotifications.Add(notification);
        await _db.SaveChangesAsync();

        // Push real-time update via SignalR to all connections of this user.
        await _hub.Clients.User(recipientId.ToString())
            .ReceiveNotification(new NotificationHubClient.NewNotification(
                notification.Id,
                notification.Subject,
                notification.Body,
                notification.CreatedAt));

        // Send Web Push if the user has registered subscriptions.
        await TrySendPushAsync(recipientId, subject, body);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<InAppNotification>> GetNotificationsAsync(Guid userId)
    {
        return await _db.InAppNotifications
            .Where(n => n.RecipientId == userId && n.DismissedAt == null)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await GetOwnedNotificationAsync(notificationId, userId);
        if (notification.ReadAt is null)
        {
            notification.ReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task DismissAsync(Guid notificationId, Guid userId)
    {
        var notification = await GetOwnedNotificationAsync(notificationId, userId);
        notification.DismissedAt = DateTime.UtcNow;
        notification.ReadAt ??= DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task SavePushSubscriptionAsync(Guid userId, string endpoint, string p256dh, string auth)
    {
        var existing = await _db.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == endpoint);
        if (existing is not null)
        {
            // Update the keys in case they changed.
            existing.P256dh = p256dh;
            existing.Auth = auth;
            existing.PersonId = userId;
        }
        else
        {
            _db.PushSubscriptions.Add(new AppPushSubscription
            {
                Id = Guid.CreateVersion7(),
                PersonId = userId,
                Endpoint = endpoint,
                P256dh = p256dh,
                Auth = auth,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RemovePushSubscriptionAsync(Guid userId, string endpoint)
    {
        var sub = await _db.PushSubscriptions
            .FirstOrDefaultAsync(s => s.PersonId == userId && s.Endpoint == endpoint);
        if (sub is not null)
        {
            _db.PushSubscriptions.Remove(sub);
            await _db.SaveChangesAsync();
        }
    }

    private async Task TrySendPushAsync(Guid recipientId, string subject, string body)
    {
        if (!_webPushSender.IsEnabled) return;

        var subscriptions = await _db.PushSubscriptions
            .Where(s => s.PersonId == recipientId)
            .ToListAsync();

        if (subscriptions.Count == 0) return;

        var payload = JsonSerializer.Serialize(new { title = subject, body });
        var staleEndpoints = new List<string>();

        foreach (var sub in subscriptions)
        {
            try
            {
                await _webPushSender.SendAsync(sub.Endpoint, sub.P256dh, sub.Auth, payload);
            }
            catch (PushSubscriptionGoneException)
            {
                staleEndpoints.Add(sub.Endpoint);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send Web Push notification to endpoint {Endpoint}", sub.Endpoint);
            }
        }

        if (staleEndpoints.Count > 0)
        {
            var toRemove = await _db.PushSubscriptions
                .Where(s => staleEndpoints.Contains(s.Endpoint))
                .ToListAsync();
            _db.PushSubscriptions.RemoveRange(toRemove);
            await _db.SaveChangesAsync();
        }
    }

    private async Task<InAppNotification> GetOwnedNotificationAsync(Guid notificationId, Guid userId)
    {
        var notification = await _db.InAppNotifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientId == userId);
        return notification ?? throw new KeyNotFoundException($"Notification {notificationId} not found.");
    }
}
