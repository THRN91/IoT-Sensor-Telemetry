# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY IoTSensorTelemetry.csproj ./
RUN dotnet restore ./IoTSensorTelemetry.csproj

COPY . .
RUN dotnet publish IoTSensorTelemetry.csproj -c Release -o /app/publish --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Run as a non-root user for defense in depth.
RUN useradd --uid 5678 --user-group --create-home appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "IoTSensorTelemetry.dll"]
