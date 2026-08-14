namespace IoTSensorTelemetry.Models.Dtos;

/// <summary>Outbound shape for a stored telemetry reading.</summary>
public class TelemetryResponse
{
    public Guid Id { get; set; }
    public string SensorId { get; set; } = string.Empty;
    public string SensorType { get; set; } = string.Empty;
    public double Value { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>Outbound shape for a computed daily KPI.</summary>
public class DailyKpiResponse
{
    public string Date { get; set; } = string.Empty;
    public string SensorType { get; set; } = string.Empty;
    public int HighValueCount { get; set; }
    public double? AverageValue { get; set; }
    public int ReadingCount { get; set; }
}
