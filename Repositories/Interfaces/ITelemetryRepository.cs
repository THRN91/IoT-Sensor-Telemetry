using IoTSensorTelemetry.Models;

namespace IoTSensorTelemetry.Repositories.Interfaces;

public interface ITelemetryRepository
{
    TelemetryEvent Add(TelemetryEvent telemetryEvent);

    /// <summary>Returns all readings for a given sensor, newest first.</summary>
    IReadOnlyList<TelemetryEvent> GetBySensorId(string sensorId);

    /// <summary>Returns all readings whose Timestamp falls on the given UTC date, for KPI aggregation.</summary>
    IReadOnlyList<TelemetryEvent> GetByDate(DateOnly date);
}
