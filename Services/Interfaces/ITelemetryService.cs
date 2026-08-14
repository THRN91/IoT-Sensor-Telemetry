using IoTSensorTelemetry.Models;
using IoTSensorTelemetry.Models.Dtos;

namespace IoTSensorTelemetry.Services.Interfaces;

public interface ITelemetryService
{
    TelemetryEvent Ingest(TelemetryIngestRequest request);
    IReadOnlyList<TelemetryEvent> GetBySensorId(string sensorId);
}
