using System.Globalization;
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

    /// <summary>
    /// Fetches stored telemetry readings for a given sensor, newest first.
    /// Supports optional date-range filtering (fromDate/toDate, yyyy-MM-dd, inclusive)
    /// and pagination (page, pageSize).
    /// </summary>
    [HttpGet("{sensorId}")]
    [ProducesResponseType(typeof(PagedTelemetryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<PagedTelemetryResponse> GetBySensorId(
        string sensorId,
        [FromQuery] string? fromDate,
        [FromQuery] string? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // Defensive validation on route/query parameters — GET requests bypass DTO
        // validation entirely, so nothing here can be trusted without an explicit check.
        if (string.IsNullOrWhiteSpace(sensorId) || sensorId.Length > 100)
        {
            return Problem(
                title: "Invalid sensorId",
                detail: "sensorId must be a non-empty string up to 100 characters",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (page < 1)
        {
            return Problem(title: "Invalid page", detail: "page must be 1 or greater", statusCode: StatusCodes.Status400BadRequest);
        }

        if (pageSize < 1 || pageSize > 500)
        {
            return Problem(title: "Invalid pageSize", detail: "pageSize must be between 1 and 500", statusCode: StatusCodes.Status400BadRequest);
        }

        DateTimeOffset? fromUtc = null;
        DateTimeOffset? toUtc = null;

        if (!string.IsNullOrWhiteSpace(fromDate))
        {
            if (!DateOnly.TryParseExact(fromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedFrom))
            {
                return Problem(title: "Invalid fromDate", detail: "fromDate must be in yyyy-MM-dd format", statusCode: StatusCodes.Status400BadRequest);
            }
            // Start of day, inclusive.
            fromUtc = new DateTimeOffset(parsedFrom.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        }

        if (!string.IsNullOrWhiteSpace(toDate))
        {
            if (!DateOnly.TryParseExact(toDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedTo))
            {
                return Problem(title: "Invalid toDate", detail: "toDate must be in yyyy-MM-dd format", statusCode: StatusCodes.Status400BadRequest);
            }
            // End of day, inclusive.
            toUtc = new DateTimeOffset(parsedTo.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
        }

        if (fromUtc.HasValue && toUtc.HasValue && fromUtc > toUtc)
        {
            return Problem(title: "Invalid date range", detail: "fromDate must not be after toDate", statusCode: StatusCodes.Status400BadRequest);
        }

        var (items, totalCount) = _telemetryService.GetBySensorId(sensorId, fromUtc, toUtc, page, pageSize);

        var response = new PagedTelemetryResponse
        {
            Items = items.Select(r => new TelemetryResponse
            {
                Id = r.Id,
                SensorId = r.SensorId,
                SensorType = r.SensorType.ToString(),
                Value = r.Value,
                Timestamp = r.Timestamp
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return Ok(response);
    }
}
