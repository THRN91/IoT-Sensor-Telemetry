using IoTSensorTelemetry.Models;
using IoTSensorTelemetry.Repositories.Interfaces;
using IoTSensorTelemetry.Services.Interfaces;

namespace IoTSensorTelemetry.Services;

/// <summary>
/// Computes daily KPIs by grouping the day's telemetry by sensor type and aggregating.
/// Thresholds are centralized here (not scattered across the codebase) so adding a new
/// sensor type or changing a threshold is a one-place change.
/// </summary>
public class KpiService : IKpiService
{
    private readonly ITelemetryRepository _telemetryRepository;
    private readonly IKpiRepository _kpiRepository;
    private readonly ILogger<KpiService> _logger;

    // High-value thresholds per the problem statement's "Recommended Daily KPIs" table.
    private static readonly Dictionary<SensorType, double> HighValueThresholds = new()
    {
        [SensorType.Temperature] = 30.0,  // > 30°C
        [SensorType.Humidity] = 70.0,     // > 70%
        [SensorType.Pressure] = 1000.0    // > 1000 hPa
    };

    public KpiService(ITelemetryRepository telemetryRepository, IKpiRepository kpiRepository, ILogger<KpiService> logger)
    {
        _telemetryRepository = telemetryRepository;
        _kpiRepository = kpiRepository;
        _logger = logger;
    }

    public IReadOnlyList<DailyKpi> ComputeDailyKpis(DateOnly date)
    {
        var readings = _telemetryRepository.GetByDate(date);

        var kpis = readings
            .GroupBy(r => r.SensorType)
            .Select(group =>
            {
                var threshold = HighValueThresholds[group.Key];
                var values = group.Select(r => r.Value).ToList();

                return new DailyKpi
                {
                    Date = date,
                    SensorType = group.Key,
                    HighValueCount = values.Count(v => v > threshold),
                    AverageValue = values.Count > 0 ? Math.Round(values.Average(), 2) : null,
                    ReadingCount = values.Count
                };
            })
            .ToList();

        _kpiRepository.SaveMany(kpis);

        _logger.LogInformation("Computed KPIs for {Date}: {SensorTypeCount} sensor type(s), {ReadingCount} reading(s)",
            date, kpis.Count, readings.Count);

        return kpis;
    }

    public IReadOnlyList<DailyKpi> GetKpis(DateOnly date)
    {
        return _kpiRepository.GetByDate(date);
    }
}
