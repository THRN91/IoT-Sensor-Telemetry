using IoTSensorTelemetry.Models;
using IoTSensorTelemetry.Models.Dtos;

namespace IoTSensorTelemetry.Services.Interfaces;

public interface ITelemetryService
{
    TelemetryEvent Ingest(TelemetryIngestRequest request);

    (IReadOnlyList<TelemetryEvent> Items, int TotalCount) GetBySensorId(
        string sensorId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int page,
        int pageSize);
}
