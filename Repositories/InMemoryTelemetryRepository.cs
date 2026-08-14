using System.Collections.Concurrent;
using IoTSensorTelemetry.Models;
using IoTSensorTelemetry.Repositories.Interfaces;

namespace IoTSensorTelemetry.Repositories;

/// <summary>
/// In-memory store for telemetry events, keyed by server-generated Guid.
/// ConcurrentDictionary makes this safe under concurrent sensor ingestion
/// without an explicit lock. Registered as a singleton so all requests share one store.
/// </summary>
public class InMemoryTelemetryRepository : ITelemetryRepository
{
    private readonly ConcurrentDictionary<Guid, TelemetryEvent> _store = new();

    public TelemetryEvent Add(TelemetryEvent telemetryEvent)
    {
        _store[telemetryEvent.Id] = telemetryEvent;
        return telemetryEvent;
    }

    public IReadOnlyList<TelemetryEvent> GetBySensorId(string sensorId)
    {
        return _store.Values
            .Where(e => string.Equals(e.SensorId, sensorId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(e => e.Timestamp)
            .ToList();
    }

    public IReadOnlyList<TelemetryEvent> GetByDate(DateOnly date)
    {
        return _store.Values
            .Where(e => DateOnly.FromDateTime(e.Timestamp.UtcDateTime) == date)
            .ToList();
    }
}
