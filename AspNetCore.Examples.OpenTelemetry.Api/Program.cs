using AspNetCore.Examples.OpenTelemetry.TelemetryServices;
using Scalar.AspNetCore;
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(cors => cors.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    }));
}

builder.Services.AddTelemetry(telemetry =>
{
    telemetry.Add<ICustomTelemetryService, CustomTelemetryService>()
        .Configure(o =>
        {
            o.Version = "1.0";
            o.Tags = [new("Tag1", "Value1"), new("Tag2", "Value2")];
        })
        .AddToOpenTelemetryProviders();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors();
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapDefaultEndpoints();

app.MapGet("/telemetry", async (ICustomTelemetryService telemetry, int delay = 0) =>
{
    using var _ = telemetry.ActivitySource.StartActivity(name: "telemetry.sample");
    telemetry.Logger.LogInformation("Telemetry endpoint called with delay = {delay}", delay);
    await Task.Delay(delay);
    telemetry.Calls.Add(1);
    telemetry.Delay.Record(delay);
});

app.MapGet("/telemetry/unregistered", async (ITelemetry<UnregisteredTelemetryName> telemetry, int delay = 0) =>
{
    using var _ = telemetry.ActivitySource.StartActivity(name: "telemetry.unregistered");
    telemetry.Logger.LogInformation("Telemetry endpoint called with delay = {delay}", delay);
    await Task.Delay(delay);
    telemetry.Meter.CreateCounter<int>("telemetry.unregistered.calls").Add(1);
    telemetry.Meter.CreateHistogram<int>("telemetry.unregistered.delay").Record(delay);
});

app.Run();

internal interface ICustomTelemetryService : ITelemetry<TelemetryName>
{
    Counter<int> Calls { get; }
    Histogram<int> Delay { get; }
}

internal class CustomTelemetryService : Telemetry<TelemetryName>, ICustomTelemetryService
{
    public CustomTelemetryService(ILoggerFactory loggerFactory, IMeterFactory meterFactory, TelemetryOptions<CustomTelemetryService> options)
        : base(loggerFactory, meterFactory, options)
    {
        Calls = Meter.CreateCounter<int>("telemetry.sample.calls");
        Delay = Meter.CreateHistogram<int>("telemetry.sample.delay");
    }

    public Counter<int> Calls { get; }
    public Histogram<int> Delay { get; }
}

internal class TelemetryName { }

internal class UnregisteredTelemetryName { }