namespace IoTSensorTelemetry.Models;

/// <summary>
/// The set of sensor types supported by the telemetry service.
/// Restricting this to an enum is itself a validation control: any value
/// outside this set fails model binding before it ever reaches business logic.
/// </summary>
public enum SensorType
{
    Temperature,
    Humidity,
    Pressure
}
