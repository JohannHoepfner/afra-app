using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     A DTO representing a profundum enrollment entry for the student dashboard.
/// </summary>
public record DashboardProfundumEntry
{
    /// <inheritdoc cref="ProfundumSlot.Jahr"/>
    public required int Jahr { get; init; }

    /// <inheritdoc cref="ProfundumSlot.Quartal"/>
    public required ProfundumQuartal Quartal { get; init; }

    /// <inheritdoc cref="ProfundumSlot.Wochentag"/>
    public required DayOfWeek Wochentag { get; init; }

    /// <inheritdoc cref="ProfundumDefinition.Bezeichnung"/>
    public required string Bezeichnung { get; init; }

    /// <inheritdoc cref="ProfundumInstanz.Ort"/>
    public required string? Ort { get; init; }
}
