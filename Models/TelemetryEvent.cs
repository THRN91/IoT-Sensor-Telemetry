namespace IoTSensorTelemetry.Models;

/// <summary>
/// Represents one sensor reading, as stored internally.
/// The Id is server-generated so no client input is ever used as a storage key,
/// which avoids collisions and key-injection style issues.
/// </summary>
public class TelemetryEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SensorId { get; set; } = string.Empty;
    public SensorType SensorType { get; set; }
    public double Value { get; set; }
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>Server-side receipt time, useful for auditing/debugging independent of client-supplied timestamps.</summary>
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
}
