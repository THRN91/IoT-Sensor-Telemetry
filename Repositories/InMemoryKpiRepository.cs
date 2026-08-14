using System.Collections.Concurrent;
using IoTSensorTelemetry.Models;
using IoTSensorTelemetry.Repositories.Interfaces;

namespace IoTSensorTelemetry.Repositories;

/// <summary>
/// In-memory store for computed daily KPIs, keyed by (date, sensorType) so a
/// re-run of the compute job cleanly overwrites the prior result instead of duplicating rows.
/// </summary>
public class InMemoryKpiRepository : IKpiRepository
{
    private readonly ConcurrentDictionary<(DateOnly Date, SensorType SensorType), DailyKpi> _store = new();

    public void SaveMany(IEnumerable<DailyKpi> kpis)
    {
        foreach (var kpi in kpis)
        {
            _store[(kpi.Date, kpi.SensorType)] = kpi;
        }
    }

    public IReadOnlyList<DailyKpi> GetByDate(DateOnly date)
    {
        return _store.Values
            .Where(k => k.Date == date)
            .OrderBy(k => k.SensorType)
            .ToList();
    }
}
