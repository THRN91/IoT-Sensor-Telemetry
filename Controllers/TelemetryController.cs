using IoTSensorTelemetry.Models.Dtos;
using IoTSensorTelemetry.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IoTSensorTelemetry.Controllers;

[ApiController]
[Route("api/telemetry")]
[Produces("application/json")]
public class TelemetryController : ControllerBase
{
    private readonly ITelemetryService _telemetryService;

    public TelemetryController(ITelemetryService telemetryService)
    {
        _telemetryService = telemetryService;
    }

    /// <summary>Ingests a single sensor telemetry reading.</summary>
    /// <remarks>
    /// [ApiController] runs DataAnnotations validation automatically before this method body
    /// executes and returns a 400 ProblemDetails response on failure — no manual
    /// ModelState.IsValid check is needed here.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(TelemetryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<TelemetryResponse> Ingest([FromBody] TelemetryIngestRequest request)
    {
        var stored = _telemetryService.Ingest(request);

        var response = new TelemetryResponse
        {
            Id = stored.Id,
            SensorId = stored.SensorId,
            SensorType = stored.SensorType.ToString(),
            Value = stored.Value,
            Timestamp = stored.Timestamp
        };

        return CreatedAtAction(nameof(GetBySensorId), new { sensorId = response.SensorId }, response);
    }

    /// <summary>Fetches all stored telemetry readings for a given sensor, newest first.</summary>
    [HttpGet("{sensorId}")]
    [ProducesResponseType(typeof(IEnumerable<TelemetryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<IEnumerable<TelemetryResponse>> GetBySensorId(string sensorId)
    {
        // Defensive validation on the route parameter too — GET requests bypass DTO
        // validation entirely, so an unvalidated route value should never be trusted.
        if (string.IsNullOrWhiteSpace(sensorId) || sensorId.Length > 100)
        {
            return Problem(
                title: "Invalid sensorId",
                detail: "sensorId must be a non-empty string up to 100 characters",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var readings = _telemetryService.GetBySensorId(sensorId);

        var response = readings.Select(r => new TelemetryResponse
        {
            Id = r.Id,
            SensorId = r.SensorId,
            SensorType = r.SensorType.ToString(),
            Value = r.Value,
            Timestamp = r.Timestamp
        });

        return Ok(response);
    }
}
