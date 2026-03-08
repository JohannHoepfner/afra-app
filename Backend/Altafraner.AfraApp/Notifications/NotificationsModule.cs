using Altafraner.AfraApp.Notifications.API.Endpoints;
using Altafraner.AfraApp.Notifications.API.Hubs;
using Altafraner.AfraApp.Notifications.Services;
using Altafraner.AfraApp.User;
using Altafraner.Backbone.Abstractions;
using Altafraner.Backbone.EmailSchedulingModule;

namespace Altafraner.AfraApp.Notifications;

/// <summary>
///     Module that adds in-app notifications, Web Push support and a notification settings API.
/// </summary>
[DependsOn<UserModule>]
[DependsOn<DatabaseModule>]
public class NotificationsModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        // Bind VAPID configuration (optional – push is silently disabled if absent).
        services.AddOptions<VapidConfiguration>()
            .Bind(config.GetSection("VAPID"));

        // Named HttpClient used by WebPushSender to send push requests.
        services.AddHttpClient("WebPush");

        // WebPushSender handles VAPID JWT + aes128gcm encryption without any third-party library.
        services.AddScoped<WebPushSender>();

        // Register the in-app notification service.
        services.AddScoped<IInAppNotificationService, InAppNotificationService>();

        // Replace the INotificationService registration with the composite one that handles both channels.
        var emailDescriptor = services.LastOrDefault(d => d.ServiceType == typeof(INotificationService));
        if (emailDescriptor is not null)
            services.Remove(emailDescriptor);

        services.AddScoped<INotificationService, CompositeNotificationService>();
    }

    /// <inheritdoc />
    public void Configure(WebApplication app)
    {
        app.MapNotificationEndpoints();
        app.MapHub<NotificationHub>("/api/notifications/hub")
            .RequireAuthorization();
    }
}
