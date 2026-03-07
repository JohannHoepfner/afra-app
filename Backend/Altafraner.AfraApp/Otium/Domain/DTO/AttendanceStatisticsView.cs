namespace Altafraner.AfraApp.Otium.Domain.DTO;

/// <summary>
///     A DTO for the attendance statistics of a single Otium, used in the global statistics view.
/// </summary>
public record AttendanceStatisticsView
{
    /// <summary>The unique identifier of the Otium.</summary>
    public Guid OtiumId { get; set; }

    /// <summary>The name of the Otium.</summary>
    public required string Bezeichnung { get; set; }

    /// <summary>
    ///     The time-ordered attendance data points for this Otium.
    ///     Only Termine whose attendance has been checked are included.
    /// </summary>
    public required IEnumerable<AttendanceDataPoint> DataPoints { get; set; }
}

/// <summary>
///     A single data point representing the attendance relative to capacity for one Termin.
/// </summary>
public record AttendanceDataPoint
{
    /// <summary>The date of the Termin.</summary>
    public DateOnly Datum { get; set; }

    /// <summary>The number of enrolled students who were marked as present.</summary>
    public int AnzahlAnwesend { get; set; }

    /// <summary>
    ///     The capacity for this Termin.
    ///     Equal to <c>MaxEinschreibungen</c> when set; otherwise falls back to the actual
    ///     number of enrolled students.
    /// </summary>
    public int Kapazitaet { get; set; }

    /// <summary>
    ///     Attendance relative to capacity as a percentage (0–100).
    ///     Zero when <see cref="Kapazitaet" /> is zero.
    /// </summary>
    public double Auslastung { get; set; }
}
