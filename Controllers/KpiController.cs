using System.Globalization;
using IoTSensorTelemetry.Models.Dtos;
using IoTSensorTelemetry.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IoTSensorTelemetry.Controllers;

[ApiController]
[Route("api/kpi")]
[Produces("application/json")]
public class KpiController : ControllerBase
{
    private readonly IKpiService _kpiService;

    public KpiController(IKpiService kpiService)
    {
        _kpiService = kpiService;
    }

    /// <summary>Manually triggers daily KPI computation for the given date (yyyy-MM-dd), grouped by sensor type.</summary>
    [HttpPost("compute")]
    [ProducesResponseType(typeof(IEnumerable<DailyKpiResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<IEnumerable<DailyKpiResponse>> Compute([FromQuery] string date)
    {
        if (!TryParseDate(date, out var parsedDate, out var error))
        {
            return Problem(title: "Invalid date", detail: error, statusCode: StatusCodes.Status400BadRequest);
        }

        var kpis = _kpiService.ComputeDailyKpis(parsedDate);
        return Ok(kpis.Select(ToResponse));
    }

    /// <summary>Fetches previously computed KPIs for the given date (yyyy-MM-dd).</summary>
    [HttpGet("{date}")]
    [ProducesResponseType(typeof(IEnumerable<DailyKpiResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<IEnumerable<DailyKpiResponse>> GetByDate(string date)
    {
        if (!TryParseDate(date, out var parsedDate, out var error))
        {
            return Problem(title: "Invalid date", detail: error, statusCode: StatusCodes.Status400BadRequest);
        }

        var kpis = _kpiService.GetKpis(parsedDate);

        if (kpis.Count == 0)
        {
            return Problem(
                title: "No KPIs found",
                detail: $"No computed KPIs exist for {parsedDate:yyyy-MM-dd}. Trigger POST /api/kpi/compute first.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(kpis.Select(ToResponse));
    }

    // Strict parsing (exact format, no culture-dependent parsing) so malformed or
    // locale-ambiguous date strings are rejected with a clear 400 instead of silently
    // misinterpreted (e.g. 03/04 as day/month vs month/day).
    private static bool TryParseDate(string? date, out DateOnly parsedDate, out string error)
    {
        parsedDate = default;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(date))
        {
            error = "date is required in yyyy-MM-dd format";
            return false;
        }

        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out parsedDate))
        {
            error = "date must be in yyyy-MM-dd format";
            return false;
        }

        return true;
    }

    private static DailyKpiResponse ToResponse(Models.DailyKpi kpi) => new()
    {
        Date = kpi.Date.ToString("yyyy-MM-dd"),
        SensorType = kpi.SensorType.ToString(),
        HighValueCount = kpi.HighValueCount,
        AverageValue = kpi.AverageValue,
        ReadingCount = kpi.ReadingCount
    };
}
