using IoTSensorTelemetry.Models;
using IoTSensorTelemetry.Models.Dtos;
using IoTSensorTelemetry.Repositories.Interfaces;
using IoTSensorTelemetry.Services.Interfaces;

namespace IoTSensorTelemetry.Services;

/// <summary>
/// Business logic for telemetry ingestion and retrieval.
/// DTO -> domain mapping happens here, not in the controller, so the controller
/// stays a thin HTTP adapter.
/// </summary>
public class TelemetryService : ITelemetryService
{
    private readonly ITelemetryRepository _repository;
    private readonly ILogger<TelemetryService> _logger;

    public TelemetryService(ITelemetryRepository repository, ILogger<TelemetryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public TelemetryEvent Ingest(TelemetryIngestRequest request)
    {
        var telemetryEvent = new TelemetryEvent
        {
            SensorId = request.SensorId,
            SensorType = request.SensorType,
            Value = request.Value,
            Timestamp = request.Timestamp
        };

        var stored = _repository.Add(telemetryEvent);

        // Log by sensor type/id only — never log full payloads, which keeps sensitive
        // or oversized field values out of application logs.
        _logger.LogInformation(
            "Stored telemetry reading {Id} for sensor {SensorId} ({SensorType})",
            stored.Id, stored.SensorId, stored.SensorType);

        return stored;
    }

    public (IReadOnlyList<TelemetryEvent> Items, int TotalCount) GetBySensorId(
        string sensorId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int page,
        int pageSize)
    {
        return _repository.GetBySensorId(sensorId, fromUtc, toUtc, page, pageSize);
    }
}
