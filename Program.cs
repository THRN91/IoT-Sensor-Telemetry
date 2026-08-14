using IoTSensorTelemetry.Middleware;
using IoTSensorTelemetry.Repositories;
using IoTSensorTelemetry.Repositories.Interfaces;
using IoTSensorTelemetry.Services;
using IoTSensorTelemetry.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Controllers: every 4xx/5xx response uses ProblemDetails (RFC 7807) consistently.
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new ProducesAttribute("application/json"));
});

builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

// Guard against oversized request bodies (denial-of-service via huge payloads).
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1_048_576; // 1 MB
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1_048_576; // 1 MB — a single telemetry reading is tiny
});

// Repositories and services registered as singletons: the in-memory store must be shared
// across all requests for the lifetime of the process, not recreated per-request/per-scope.
builder.Services.AddSingleton<ITelemetryRepository, InMemoryTelemetryRepository>();
builder.Services.AddSingleton<IKpiRepository, InMemoryKpiRepository>();
builder.Services.AddSingleton<ITelemetryService, TelemetryService>();
builder.Services.AddSingleton<IKpiService, KpiService>();

// CORS explicitly scoped and named so it's easy to tighten for a real deployment,
// rather than a bare "*" wildcard.
const string CorsPolicy = "DefaultCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy.WithMethods("GET", "POST")
              .AllowAnyHeader()
              .SetIsOriginAllowed(_ => true); // relax for local/demo use; restrict to known origins in production
    });
});

var app = builder.Build();

// Global exception handling first in the pipeline so it wraps everything below it.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);
app.UseAuthorization();

app.MapControllers();

// Lightweight root endpoint so hitting "/" gives something useful without Swagger
// (Swashbuckle was omitted because this sandbox cannot reach nuget.org — see README
// for the one-line change to add Swagger back in a normal environment).
app.MapGet("/", () => Results.Ok(new
{
    service = "IoT Sensor Telemetry Service",
    endpoints = new[]
    {
        "POST /api/telemetry",
        "GET  /api/telemetry/{sensorId}",
        "POST /api/kpi/compute?date=yyyy-MM-dd",
        "GET  /api/kpi/{date}"
    }
}));

app.Run();

// Exposed for WebApplicationFactory-based integration testing.
public partial class Program { }
