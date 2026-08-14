# Sample Output:
**GET:**
<img width="842" height="486" alt="image" src="https://github.com/user-attachments/assets/b1adb9af-3d9b-4f99-ac2e-d7b8c6ff3f66" />

**Post**
<img width="939" height="336" alt="image" src="https://github.com/user-attachments/assets/ad2a01ed-5936-4f4f-91c3-80b01c0fe58c" />

# IoT Sensor Telemetry Service

A lightweight REST API microservice that ingests IoT sensor telemetry (Temperature,
Humidity, Pressure), stores it in memory, computes daily KPIs, and exposes endpoints
to retrieve both raw telemetry and computed KPIs.

Built for the *IoT Sensor Telemetry* hackathon problem statement.

## Tech stack

- **ASP.NET Core Web API (.NET 8)**, controller-based
- **In-memory storage** — `ConcurrentDictionary`-backed repositories (thread-safe, no external DB)
- **DataAnnotations** for request validation
- Layered architecture: `Controllers -> Services -> Repositories -> In-memory store`

> Note: this build does not include Swagger/Swashbuckle because the sandbox this was
> built in could not reach `nuget.org`. To add Swagger in a normal environment:
> `dotnet add package Swashbuckle.AspNetCore`, then re-add
> `builder.Services.AddSwaggerGen()` / `app.UseSwagger(); app.UseSwaggerUI();` in
> `Program.cs` (the original template lines are noted there).

## Project structure

```
IoTSensorTelemetry/
├── Controllers/         # Thin HTTP layer: TelemetryController, KpiController
├── Services/            # Business logic: ingestion, KPI aggregation
├── Repositories/        # Data access abstraction over the in-memory store
├── Models/               
│   └── Dtos/             # Request/response DTOs, separate from domain models
├── Middleware/           # Global exception handling (sanitized error responses)
└── Program.cs            # DI wiring, security hardening, pipeline
```

## Setup & run

Requires the .NET 8 SDK.

```bash
cd IoTSensorTelemetry
dotnet restore
dotnet build
dotnet run
```

The API listens on the URL printed at startup (typically `http://localhost:5000` or
`http://localhost:5xxx` — check the console output). Hitting `GET /` lists all
available endpoints.

## API endpoints

| Method | Route                              | Purpose                                   |
|--------|-------------------------------------|--------------------------------------------|
| POST   | `/api/telemetry`                    | Ingest a telemetry reading                |
| GET    | `/api/telemetry/{sensorId}`         | Fetch readings for a sensor — paginated, with optional date-range filter |
| POST   | `/api/kpi/compute?date=yyyy-MM-dd`  | Compute (and persist) daily KPIs          |
| GET    | `/api/kpi/{date}`                   | Fetch previously computed KPIs (`yyyy-MM-dd`) |

**`GET /api/telemetry/{sensorId}` query parameters** (all optional):

| Param      | Default | Notes                                              |
|------------|---------|-----------------------------------------------------|
| `fromDate` | —       | `yyyy-MM-dd`, inclusive, start of that UTC day       |
| `toDate`   | —       | `yyyy-MM-dd`, inclusive, end of that UTC day         |
| `page`     | `1`     | 1-based page number                                 |
| `pageSize` | `20`    | 1–500                                                |

## Running with Docker

**Quickest — one command via Docker Compose:**
```bash
docker compose up
```
Then the API is reachable at `http://localhost:8080`.

**Or manually:**
```bash
docker build -t iot-telemetry .
docker run -p 8080:8080 iot-telemetry
```

The image uses a multi-stage build (SDK to publish, `aspnet:8.0` runtime to run) and runs
as a non-root user.

**Sharing with others without giving them the source:** push the built image to a registry
(e.g. Docker Hub) once, and anyone can then run it with a single command and no local build:
```bash
docker build -t yourusername/iot-telemetry:latest .
docker login
docker push yourusername/iot-telemetry:latest

# anyone else then just runs:
docker run -p 8080:8080 yourusername/iot-telemetry:latest
```

> This Dockerfile and `docker-compose.yml` were written and reviewed but **not build-tested**
> — the sandbox this project was built in has no Docker daemon and cannot reach
> `mcr.microsoft.com` to pull base images. They follow the standard, well-established
> ASP.NET Core containerization pattern, but please verify the build in an environment with
> Docker and outbound internet access before relying on it or sharing it further.

## Sample requests

**Ingest a reading**
```bash
curl -X POST http://localhost:5177/api/telemetry \
  -H "Content-Type: application/json" \
  -d '{
    "sensorId": "sensor-1",
    "sensorType": "Temperature",
    "value": 32.5,
    "timestamp": "2026-08-14T02:00:00Z"
  }'
```
`sensorType` must be exactly one of `Temperature`, `Humidity`, `Pressure`. Returns `201 Created`.

**Fetch readings for a sensor (paginated)**
```bash
curl "http://localhost:5177/api/telemetry/sensor-1?page=1&pageSize=10"
```

**Fetch readings within a date range**
```bash
curl "http://localhost:5177/api/telemetry/sensor-1?fromDate=2026-08-01&toDate=2026-08-14&page=1&pageSize=50"
```
Response shape:
```json
{
  "items": [ { "id": "...", "sensorId": "sensor-1", "sensorType": "Temperature", "value": 32.5, "timestamp": "..." } ],
  "page": 1,
  "pageSize": 50,
  "totalCount": 3,
  "totalPages": 1
}
```

**Trigger KPI computation for a date**
```bash
curl -X POST "http://localhost:5177/api/kpi/compute?date=2026-08-14"
```

**Fetch computed KPIs for a date**
```bash
curl http://localhost:5177/api/kpi/2026-08-14
```
Returns `404` with a `ProblemDetails` body if KPIs haven't been computed yet for that date.

## KPI definitions

| Sensor type | High-value threshold | KPIs computed              |
|-------------|-----------------------|------------------------------|
| Temperature | > 30°C                | high-value count, daily average |
| Humidity    | > 70%                 | high-value count, daily average |
| Pressure    | > 1000 hPa            | high-value count, daily average |

KPI computation is idempotent per `(date, sensorType)` — re-running `POST /api/kpi/compute`
for a date recomputes and overwrites the prior result rather than duplicating rows.

## Validation & error handling

- `sensorId`: required, 1–100 chars, restricted to `[a-zA-Z0-9_-]` (blocks control
  characters / injection-style payloads in a value that gets stored and echoed back)
- `sensorType`: must bind to the `Temperature | Humidity | Pressure` enum — any other
  value fails model binding with a clear `400`
- `value`: required, numeric, bounded to a realistic sensor range
- `timestamp`: required, valid date-time, rejected if implausibly far in the future or past
- `page` / `pageSize` / `fromDate` / `toDate` on the telemetry fetch endpoint are validated
  independently (page ≥ 1, 1 ≤ pageSize ≤ 500, dates in strict `yyyy-MM-dd` format, `fromDate`
  not after `toDate`) since query parameters bypass DTO model validation entirely
- All validation failures return `400` with an RFC 7807 `ProblemDetails` body
- Unhandled exceptions are caught by global middleware, logged server-side, and returned
  to the client as a sanitized `500 ProblemDetails` (no stack traces or internal details
  are ever leaked in the response)
- Request bodies are capped at 1 MB (Kestrel `MaxRequestBodySize`) to guard against
  oversized-payload abuse
- CORS is explicitly configured (named policy, GET/POST only) rather than left wide open

## Data model

**TelemetryEvent** — `Id`, `SensorId`, `SensorType`, `Value`, `Timestamp`, `ReceivedAt`

**DailyKpi** — `Date`, `SensorType`, `HighValueCount`, `AverageValue`, `ReadingCount`

## Design notes / assumptions

- Storage is in-memory only, per the "keep it simple" scope of the exercise — data is
  lost on restart. Repositories are behind interfaces (`ITelemetryRepository`,
  `IKpiRepository`) so swapping in SQLite later is a contained change.
- KPI computation is manually triggered via `POST /api/kpi/compute`, as permitted by the
  problem statement for a 1-hour exercise (rather than a background scheduled job).
- No authentication was added — the problem statement doesn't call for it and the exercise
  is explicitly scoped to REST API design, validation, and aggregation logic. For a
  production deployment, add API key or JWT auth, rate limiting, and audit logging.
- Not implemented (explicitly out of scope for this pass): unit tests, SQLite persistence.

## Manual test coverage performed

- Valid ingest → `201 Created`
- Missing `sensorId` → `400`
- Invalid `sensorType` (not in enum) → `400`
- Future timestamp → `400`
- `sensorId` with disallowed characters → `400`
- Fetch by sensor ID (existing and non-existent) → `200` with correct/empty results
- Pagination: 25 seeded readings, `pageSize=20` → page 1 returns 20, page 2 returns remaining 5, `totalCount`/`totalPages` correct
- Date-range filtering: single-day range and multi-day range both return the correct subset
- Invalid `fromDate` format, `fromDate` after `toDate`, out-of-range `page`/`pageSize` → all `400`
- KPI compute → correct per-sensor-type aggregation and high-value counts
- KPI fetch for a date with no data → `404`
