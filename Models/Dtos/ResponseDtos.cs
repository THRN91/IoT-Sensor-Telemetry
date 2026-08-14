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

/// <summary>Outbound shape for a page of telemetry results, with paging metadata.</summary>
public class PagedTelemetryResponse
{
    public IReadOnlyList<TelemetryResponse> Items { get; set; } = Array.Empty<TelemetryResponse>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
