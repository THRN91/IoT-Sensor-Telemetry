namespace IoTSensorTelemetry.Models;

/// <summary>
/// Represents the computed KPI summary for a single sensor type on a single day.
/// </summary>
public class DailyKpi
{
    public DateOnly Date { get; set; }
    public SensorType SensorType { get; set; }
    public int HighValueCount { get; set; }
    public double? AverageValue { get; set; }
    public int ReadingCount { get; set; }
}
