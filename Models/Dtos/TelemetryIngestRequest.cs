using System.ComponentModel.DataAnnotations;
using IoTSensorTelemetry.Models;

namespace IoTSensorTelemetry.Models.Dtos;

/// <summary>
/// Inbound payload for POST /api/telemetry.
/// Every field is validated via DataAnnotations before it reaches the service layer —
/// controllers never hand unvalidated input to business logic.
/// </summary>
public class TelemetryIngestRequest : IValidatableObject
{
    [Required(ErrorMessage = "sensorId is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "sensorId must be between 1 and 100 characters")]
    // Restrict to a safe character set: letters, digits, dash, underscore.
    // This blocks control characters, path separators, and script-injection style payloads
    // from ever being stored or reflected back in a response.
    [RegularExpression(@"^[a-zA-Z0-9_\-]+$", ErrorMessage = "sensorId may only contain letters, digits, '-' and '_'")]
    public string SensorId { get; set; } = string.Empty;

    // Binding straight to the enum means an unrecognized string (e.g. "temperature" lowercase,
    // or "Voltage") fails model binding automatically with a clear 400 — no manual parsing needed.
    [Required(ErrorMessage = "sensorType is required")]
    [EnumDataType(typeof(SensorType), ErrorMessage = "sensorType must be one of Temperature, Humidity, Pressure")]
    public SensorType SensorType { get; set; }

    [Required(ErrorMessage = "value is required")]
    // Bounds guard against garbage/overflow values (e.g. NaN cannot bind, but absurd magnitudes can).
    [Range(-10000, 10000, ErrorMessage = "value must be a realistic numeric sensor reading")]
    public double Value { get; set; }

    [Required(ErrorMessage = "timestamp is required")]
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Cross-field / semantic validation that a single attribute can't express:
    /// timestamps must be a real, sane date-time — not far in the future (clock skew tolerance)
    /// and not implausibly old. This is a data-quality guard, not just a type check.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var now = DateTimeOffset.UtcNow;

        if (Timestamp > now.AddMinutes(5))
        {
            yield return new ValidationResult(
                "timestamp cannot be in the future",
                new[] { nameof(Timestamp) });
        }

        if (Timestamp < now.AddYears(-5))
        {
            yield return new ValidationResult(
                "timestamp is implausibly old",
                new[] { nameof(Timestamp) });
        }
    }
}
