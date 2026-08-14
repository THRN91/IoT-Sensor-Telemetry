using IoTSensorTelemetry.Models;

namespace IoTSensorTelemetry.Repositories.Interfaces;

public interface IKpiRepository
{
    /// <summary>Upserts KPI rows for a date — recomputation replaces any prior result for that date+sensorType.</summary>
    void SaveMany(IEnumerable<DailyKpi> kpis);

    IReadOnlyList<DailyKpi> GetByDate(DateOnly date);
}
