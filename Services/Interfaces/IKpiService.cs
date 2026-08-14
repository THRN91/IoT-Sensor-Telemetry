using IoTSensorTelemetry.Models;

namespace IoTSensorTelemetry.Services.Interfaces;

public interface IKpiService
{
    /// <summary>Computes and persists daily KPIs for every sensor type for the given date. Returns the computed rows.</summary>
    IReadOnlyList<DailyKpi> ComputeDailyKpis(DateOnly date);

    IReadOnlyList<DailyKpi> GetKpis(DateOnly date);
}
