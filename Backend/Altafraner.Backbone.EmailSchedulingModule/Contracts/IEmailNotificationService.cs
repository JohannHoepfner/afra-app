namespace Altafraner.Backbone.EmailSchedulingModule;

/// <summary>
///     A service that delivers notifications via e-mail.
///     Identical contract to <see cref="INotificationService" />; the separate interface
///     allows the composite notification service to reference only the e-mail delivery
///     without creating a circular dependency on <see cref="INotificationService" />.
/// </summary>
public interface IEmailNotificationService
{
    /// <summary>
    ///     Schedule an e-mail notification for delivery in a batch within the specified timeframe.
    /// </summary>
    Task ScheduleNotificationAsync(Guid recipientId, string subject, string body, TimeSpan deadline);
}
