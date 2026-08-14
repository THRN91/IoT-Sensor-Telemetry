using IoTSensorTelemetry.Models;

namespace IoTSensorTelemetry.Repositories.Interfaces;

public interface ITelemetryRepository
{
    TelemetryEvent Add(TelemetryEvent telemetryEvent);

    /// <summary>
    /// Returns readings for a sensor, newest first, optionally restricted to a date range
    /// (inclusive) and paged. Filtering and paging happen here (not in the controller) so the
    /// service/controller layers never hold the full unbounded collection in memory at once.
    /// </summary>
    (IReadOnlyList<TelemetryEvent> Items, int TotalCount) GetBySensorId(
        string sensorId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int page,
        int pageSize);

    /// <summary>Returns all readings whose Timestamp falls on the given UTC date, for KPI aggregation.</summary>
    IReadOnlyList<TelemetryEvent> GetByDate(DateOnly date);
}
