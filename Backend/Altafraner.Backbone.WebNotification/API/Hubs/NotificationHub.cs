using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Altafraner.Backbone.WebNotifications.API.Hubs;

/// <summary>
///     A SignalR hub that delivers real-time in-app notifications to authenticated users.
/// </summary>
[Authorize]
public class NotificationHub : Hub<INotificationHubClient>
{
    // Clients connect automatically and are identified by their user id via ClaimTypes.NameIdentifier.
    // The server pushes notifications to specific users via IHubContext<NotificationHub, INotificationHubClient>.
}
